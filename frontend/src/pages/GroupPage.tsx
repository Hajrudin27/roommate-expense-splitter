import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { http } from "../api/client";
import type {
  CreateExpenseRequest,
  CreatePaymentRequest,
  GroupSummaryResponse,
} from "../types/api";

function shortId(id: string) {
  return id.length > 8 ? `${id.slice(0, 8)}…` : id;
}

function collectKnownUserIds(data: GroupSummaryResponse): string[] {
  const ids = new Set<string>();

  data.balances.netBalances.forEach((b) => ids.add(b.userId));
  data.balances.transfers.forEach((t) => {
    ids.add(t.fromUserId);
    ids.add(t.toUserId);
  });

  data.expenses.forEach((e) => {
    ids.add(e.paidByUserId);
    e.shares.forEach((s) => ids.add(s.userId));
  });

  data.payments.forEach((p) => {
    ids.add(p.fromUserId);
    ids.add(p.toUserId);
  });

  return Array.from(ids).sort();
}

export default function GroupPage() {
  const { groupId } = useParams();
  const [data, setData] = useState<GroupSummaryResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // --- Add Expense form state ---
  const [desc, setDesc] = useState("");
  const [amount, setAmount] = useState<string>(""); // keep as string for nicer UX
  const [paidByUserId, setPaidByUserId] = useState("");
  const [expenseDate, setExpenseDate] = useState(() =>
    new Date().toISOString().slice(0, 10)
  );

  // participants (checkbox list)
  const [participantUserIds, setParticipantUserIds] = useState<string[]>([]);

  // --- Add Payment form state ---
  const [fromUserId, setFromUserId] = useState("");
  const [toUserId, setToUserId] = useState("");
  const [paymentAmount, setPaymentAmount] = useState<string>("");
  const [paymentDate, setPaymentDate] = useState(() =>
    new Date().toISOString().slice(0, 10)
  );

  // --- UI-only user labels (stable per group, stored in localStorage) ---
  const [userLabels, setUserLabels] = useState<Record<string, string>>({});

  const knownUserIds = useMemo(() => {
    if (!data) return [];
    return collectKnownUserIds(data);
  }, [data]);

  function labelOf(userId: string) {
    return userLabels[userId] ?? `User (${shortId(userId)})`;
  }

  function storageKey() {
    return `rm-splitter:user-labels:${groupId ?? "unknown"}`;
  }

  async function load() {
    if (!groupId) return;
    setError(null);
    setIsLoading(true);

    try {
      const summary = await http<GroupSummaryResponse>(
        `/api/groups/${groupId}/summary`
      );
      setData(summary);
    } catch (e: any) {
      setError(e?.message ?? "Failed to load group summary");
    } finally {
      setIsLoading(false);
    }
  }

  // initial load
  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [groupId]);

  // initialize / merge labels + default participants whenever data changes
  useEffect(() => {
    if (!groupId || !data) return;

    const ids = collectKnownUserIds(data);

    // load saved labels
    let saved: Record<string, string> = {};
    try {
      const raw = localStorage.getItem(storageKey());
      if (raw) saved = JSON.parse(raw);
    } catch {
        // ignore
    }

    // ensure every id has a label
    const merged: Record<string, string> = { ...saved };
    let nextIndex = 1;

    // keep existing numbering stable: count existing "User N"
    const usedNumbers = new Set<number>();
    Object.values(merged).forEach((v) => {
      const m = /^User (\d+)$/.exec(v);
      if (m) usedNumbers.add(Number(m[1]));
    });
    while (usedNumbers.has(nextIndex)) nextIndex++;

    for (const id of ids) {
      if (!merged[id]) {
        merged[id] = `User ${nextIndex}`;
        usedNumbers.add(nextIndex);
        do nextIndex++;
        while (usedNumbers.has(nextIndex));
      }
    }

    setUserLabels(merged);
    localStorage.setItem(storageKey(), JSON.stringify(merged));

    // default: participants = all known users (nice for “equal split”)
    setParticipantUserIds((prev) => (prev.length ? prev : ids));

    // default selects if empty
    setPaidByUserId((prev) => prev || ids[0] || "");
    setFromUserId((prev) => prev || ids[0] || "");
    setToUserId((prev) => prev || ids[1] || ids[0] || "");
  }, [data, groupId]);

  function updateUserLabel(userId: string, newLabel: string) {
    const next = { ...userLabels, [userId]: newLabel };
    setUserLabels(next);
    try {
      localStorage.setItem(storageKey(), JSON.stringify(next));
    } catch {
      // ignore
    }
  }

  function toggleParticipant(userId: string) {
    setParticipantUserIds((prev) =>
      prev.includes(userId) ? prev.filter((x) => x !== userId) : [...prev, userId]
    );
  }

  async function onCreateExpense(e: React.FormEvent) {
    e.preventDefault();
    if (!groupId) return;

    setError(null);

    const body: CreateExpenseRequest = {
      description: desc.trim(),
      amount: Number(amount),
      paidByUserId: paidByUserId.trim(),
      expenseDate,
      participantUserIds,
    };

    try {
      await http(`/api/groups/${groupId}/expenses`, {
        method: "POST",
        body: JSON.stringify(body),
      });

      setDesc("");
      setAmount("");
      // keep paidBy + participants for rapid entry
      await load();
    } catch (err: any) {
      setError(err?.message ?? "Failed to create expense");
    }
  }

  async function onCreatePayment(e: React.FormEvent) {
    e.preventDefault();
    if (!groupId) return;

    setError(null);

    const body: CreatePaymentRequest = {
      fromUserId: fromUserId.trim(),
      toUserId: toUserId.trim(),
      amount: Number(paymentAmount),
      paymentDate,
    };

    try {
      await http(`/api/groups/${groupId}/payments`, {
        method: "POST",
        body: JSON.stringify(body),
      });

      setPaymentAmount("");
      await load();
    } catch (err: any) {
      setError(err?.message ?? "Failed to create payment");
    }
  }

  if (!groupId) return <div>Missing groupId</div>;

  return (
    <div style={{ maxWidth: 1000, margin: "40px auto", padding: 16 }}>
      <Link to="/">← Back</Link>

      {error && (
        <pre style={{ color: "crimson", whiteSpace: "pre-wrap" }}>{error}</pre>
      )}
      {isLoading && <div>Loading…</div>}

      {data && (
        <>
          <h1 style={{ marginBottom: 8 }}>{data.group.name}</h1>
          <p style={{ marginTop: 0, opacity: 0.85 }}>
            Currency: <b>{data.group.currency}</b> • Created:{" "}
            {new Date(data.group.createdAt).toLocaleString()}
          </p>

          {/* Users */}
          <h2>Users (UI labels)</h2>
          <div style={{ display: "grid", gap: 8, marginBottom: 24 }}>
            {knownUserIds.length === 0 ? (
              <div style={{ opacity: 0.8 }}>
                No users found yet. Add an expense/payment and the UI will learn the GUIDs.
              </div>
            ) : (
              knownUserIds.map((id) => (
                <label key={id} style={{ display: "flex", gap: 8, alignItems: "center" }}>
                  <span style={{ width: 140, fontFamily: "monospace", fontSize: 12, opacity: 0.8 }}>
                    {shortId(id)}
                  </span>
                  <input
                    value={userLabels[id] ?? ""}
                    onChange={(e) => updateUserLabel(id, e.target.value)}
                    style={{ flex: 1, padding: 8 }}
                  />
                </label>
              ))
            )}
          </div>

          {/* Add Expense */}
          <h2>Add Expense</h2>
          <form onSubmit={onCreateExpense} style={{ display: "grid", gap: 8, marginBottom: 24 }}>
            <input
              placeholder="Description (e.g. Dinner)"
              value={desc}
              onChange={(e) => setDesc(e.target.value)}
              style={{ padding: 8 }}
              required
            />

            <input
              type="number"
              step="0.01"
              placeholder="Amount"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              style={{ padding: 8 }}
              min={0}
              required
            />

            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 12, opacity: 0.8 }}>Paid by</span>
              <select
                value={paidByUserId}
                onChange={(e) => setPaidByUserId(e.target.value)}
                style={{ padding: 8 }}
                required
                disabled={knownUserIds.length === 0}
              >
                {knownUserIds.map((id) => (
                  <option key={id} value={id}>
                    {labelOf(id)} ({shortId(id)})
                  </option>
                ))}
              </select>
            </label>

            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 12, opacity: 0.8 }}>Expense date</span>
              <input
                type="date"
                value={expenseDate}
                onChange={(e) => setExpenseDate(e.target.value)}
                style={{ padding: 8 }}
                required
              />
            </label>

            <div style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 12, opacity: 0.8 }}>Participants</span>
              <div style={{ display: "grid", gap: 6, padding: 8, border: "1px solid #ccc", borderRadius: 8 }}>
                {knownUserIds.map((id) => {
                  const checked = participantUserIds.includes(id);
                  return (
                    <label key={id} style={{ display: "flex", gap: 8, alignItems: "center" }}>
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={() => toggleParticipant(id)}
                      />
                      <span>
                        {labelOf(id)} <span style={{ fontFamily: "monospace", fontSize: 12, opacity: 0.7 }}>({shortId(id)})</span>
                      </span>
                    </label>
                  );
                })}
              </div>
              <div style={{ fontSize: 12, opacity: 0.8 }}>
                Selected: {participantUserIds.length}
              </div>
            </div>

            <button
              type="submit"
              style={{ padding: "10px 12px" }}
              disabled={knownUserIds.length === 0 || participantUserIds.length === 0}
            >
              Create Expense
            </button>
          </form>

          {/* Add Payment */}
          <h2>Add Payment</h2>
          <form onSubmit={onCreatePayment} style={{ display: "grid", gap: 8, marginBottom: 24 }}>
            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 12, opacity: 0.8 }}>From</span>
              <select
                value={fromUserId}
                onChange={(e) => setFromUserId(e.target.value)}
                style={{ padding: 8 }}
                required
                disabled={knownUserIds.length === 0}
              >
                {knownUserIds.map((id) => (
                  <option key={id} value={id}>
                    {labelOf(id)} ({shortId(id)})
                  </option>
                ))}
              </select>
            </label>

            <label style={{ display: "grid", gap: 6 }}>
              <span style={{ fontSize: 12, opacity: 0.8 }}>To</span>
              <select
                value={toUserId}
                onChange={(e) => setToUserId(e.target.value)}
                style={{ padding: 8 }}
                required
                disabled={knownUserIds.length === 0}
              >
                {knownUserIds.map((id) => (
                  <option key={id} value={id}>
                    {labelOf(id)} ({shortId(id)})
                  </option>
                ))}
              </select>
            </label>

            <input
              type="number"
              step="0.01"
              placeholder="Amount"
              value={paymentAmount}
              onChange={(e) => setPaymentAmount(e.target.value)}
              style={{ padding: 8 }}
              min={0}
              required
            />

            <input
              type="date"
              value={paymentDate}
              onChange={(e) => setPaymentDate(e.target.value)}
              style={{ padding: 8 }}
              required
            />

            <button type="submit" style={{ padding: "10px 12px" }} disabled={knownUserIds.length === 0}>
              Create Payment
            </button>
          </form>

          <h2>Balances</h2>
          <ul>
            {data.balances.netBalances.map((b) => (
              <li key={b.userId}>
                <b>{labelOf(b.userId)}</b>{" "}
                <span style={{ fontFamily: "monospace", fontSize: 12, opacity: 0.7 }}>
                  ({shortId(b.userId)})
                </span>{" "}
                :{" "}
                <b style={{ color: b.amount >= 0 ? "green" : "crimson" }}>
                  {b.amount.toFixed(2)}
                </b>
              </li>
            ))}
          </ul>

          <h3>Suggested Transfers</h3>
          {data.balances.transfers.length === 0 ? (
            <p>No transfers needed.</p>
          ) : (
            <ul>
              {data.balances.transfers.map((t, idx) => (
                <li key={idx}>
                  <b>{labelOf(t.fromUserId)}</b> → <b>{labelOf(t.toUserId)}</b> :{" "}
                  <b>{t.amount.toFixed(2)}</b>
                </li>
              ))}
            </ul>
          )}

          <h2>Expenses</h2>
          <ul>
            {data.expenses.map((e) => (
              <li key={e.id}>
                <b>{e.description}</b> — {e.amount.toFixed(2)} — paid by{" "}
                <b>{labelOf(e.paidByUserId)}</b> — {e.expenseDate}
              </li>
            ))}
          </ul>

          <h2>Payments</h2>
          <ul>
            {data.payments.map((p) => (
              <li key={p.id}>
                <b>{labelOf(p.fromUserId)}</b> → <b>{labelOf(p.toUserId)}</b> —{" "}
                {p.amount.toFixed(2)} — {p.paymentDate}
              </li>
            ))}
          </ul>
        </>
      )}
    </div>
  );
}
