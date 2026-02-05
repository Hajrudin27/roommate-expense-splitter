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

  // balances
  data.balances.netBalances.forEach((b) => ids.add(b.userId));
  data.balances.transfers.forEach((t) => {
    ids.add(t.fromUserId);
    ids.add(t.toUserId);
  });

  // expenses
  data.expenses.forEach((e) => {
    ids.add(e.paidByUserId);
    e.shares.forEach((s) => ids.add(s.userId));
  });

  // payments
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

  // UI-only user labels
  const [userLabels, setUserLabels] = useState<Record<string, string>>({});

  // Add Expense form
  const [desc, setDesc] = useState("");
  const [amount, setAmount] = useState<string>(""); // string = nicer input UX
  const [paidByUserId, setPaidByUserId] = useState("");
  const [expenseDate, setExpenseDate] = useState(() =>
    new Date().toISOString().slice(0, 10)
  );
  const [participantUserIds, setParticipantUserIds] = useState<string[]>([]);

  // Add Payment form
  const [fromUserId, setFromUserId] = useState("");
  const [toUserId, setToUserId] = useState("");
  const [paymentAmount, setPaymentAmount] = useState<string>("");
  const [paymentDate, setPaymentDate] = useState(() =>
    new Date().toISOString().slice(0, 10)
  );

  const knownUserIds = useMemo(() => {
    if (!data) return [];
    return collectKnownUserIds(data);
  }, [data]);

  function storageKey() {
    return `rm-splitter:user-labels:${groupId ?? "unknown"}`;
  }

  function labelOf(userId: string) {
    return userLabels[userId] ?? `User (${shortId(userId)})`;
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

  useEffect(() => {
    load();
  }, [groupId]);

  useEffect(() => {
    if (!groupId || !data) return;

    const ids = collectKnownUserIds(data);

    // Load saved labels
    let saved: Record<string, string> = {};
    try {
      const raw = localStorage.getItem(storageKey());
      if (raw) saved = JSON.parse(raw);
    } catch {}

    const merged: Record<string, string> = { ...saved };

    const usedNumbers = new Set<number>();
    Object.values(merged).forEach((v) => {
      const m = /^User (\d+)$/.exec(v);
      if (m) usedNumbers.add(Number(m[1]));
    });

    let next = 1;
    while (usedNumbers.has(next)) next++;

    for (const id of ids) {
      if (!merged[id]) {
        merged[id] = `User ${next}`;
        usedNumbers.add(next);
        do next++;
        while (usedNumbers.has(next));
      }
    }

    setUserLabels(merged);
    localStorage.setItem(storageKey(), JSON.stringify(merged));

    setParticipantUserIds((prev) => (prev.length ? prev : ids));
 
    setPaidByUserId((prev) => prev || ids[0] || "");
    setFromUserId((prev) => prev || ids[0] || "");
    setToUserId((prev) => prev || ids[1] || ids[0] || "");
  }, [data, groupId]);

  function updateUserLabel(userId: string, newLabel: string) {
    const next = { ...userLabels, [userId]: newLabel };
    setUserLabels(next);
    try {
      localStorage.setItem(storageKey(), JSON.stringify(next));
    } catch {}
  }

  function toggleParticipant(userId: string) {
    setParticipantUserIds((prev) =>
      prev.includes(userId)
        ? prev.filter((x) => x !== userId)
        : [...prev, userId]
    );
  }

  async function onCreateExpense(e: React.FormEvent) {
    e.preventDefault();
    if (!groupId) return;

    setError(null);

    const body: CreateExpenseRequest = {
      description: desc.trim(),
      amount: Number(amount),
      paidByUserId,
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
      fromUserId,
      toUserId,
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
    <div className="stack">
      <Link to="/" className="muted">
        ← Back
      </Link>

      {error && <div className="errorBox">{error}</div>}
      {isLoading && <div className="muted">Loading…</div>}

      {data && (
        <>
          <section className="card stack">
            <div className="stack" style={{ gap: 6 }}>
              <h1>{data.group.name}</h1>
              <p className="muted">
                Currency: <b>{data.group.currency}</b> • Created:{" "}
                {new Date(data.group.createdAt).toLocaleString()}
              </p>
            </div>
          </section>

          <section className="card stack">
            <h2>Users (UI labels)</h2>

            {knownUserIds.length === 0 ? (
              <div className="muted">
                No users found yet. Add an expense/payment and the UI will learn the GUIDs.
              </div>
            ) : (
              <div className="stack" style={{ gap: 8 }}>
                {knownUserIds.map((id) => (
                  <label
                    key={id}
                    style={{ display: "grid", gridTemplateColumns: "140px 1fr", gap: 10 }}
                  >
                    <span
                      className="muted"
                      style={{ fontFamily: "monospace", fontSize: 12, alignSelf: "center" }}
                      title={id}
                    >
                      {shortId(id)}
                    </span>
                    <input
                      value={userLabels[id] ?? ""}
                      onChange={(e) => updateUserLabel(id, e.target.value)}
                    />
                  </label>
                ))}
              </div>
            )}
          </section>

          <section className="card stack">
            <h2>Add Expense</h2>

            <form onSubmit={onCreateExpense} className="stack">
              <input
                placeholder="Description (e.g. Dinner)"
                value={desc}
                onChange={(e) => setDesc(e.target.value)}
                required
              />

              <input
                type="number"
                step="0.01"
                placeholder="Amount"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                min={0}
                required
              />

              <label>
                <span className="muted" style={{ fontSize: 12 }}>
                  Paid by
                </span>
                <select
                  value={paidByUserId}
                  onChange={(e) => setPaidByUserId(e.target.value)}
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

              <label>
                <span className="muted" style={{ fontSize: 12 }}>
                  Expense date
                </span>
                <input
                  type="date"
                  value={expenseDate}
                  onChange={(e) => setExpenseDate(e.target.value)}
                  required
                />
              </label>

              <div className="stack" style={{ gap: 8 }}>
                <div className="muted" style={{ fontSize: 12 }}>
                  Participants (equal split)
                </div>

                <div
                  className="stack"
                  style={{
                    gap: 6,
                    padding: 12,
                    border: "1px solid #d7dbe3",
                    borderRadius: 12,
                    background: "white",
                  }}
                >
                  {knownUserIds.map((id) => (
                    <label
                      key={id}
                      style={{ display: "flex", gap: 10, alignItems: "center" }}
                    >
                      <input
                        type="checkbox"
                        checked={participantUserIds.includes(id)}
                        onChange={() => toggleParticipant(id)}
                        style={{ width: 16, height: 16 }}
                      />
                      <span>
                        {labelOf(id)}{" "}
                        <span
                          className="muted"
                          style={{ fontFamily: "monospace", fontSize: 12 }}
                        >
                          ({shortId(id)})
                        </span>
                      </span>
                    </label>
                  ))}
                </div>

                <div className="muted" style={{ fontSize: 12 }}>
                  Selected: {participantUserIds.length}
                </div>
              </div>

              <button
                type="submit"
                disabled={
                  knownUserIds.length === 0 ||
                  participantUserIds.length === 0 ||
                  !desc.trim() ||
                  !amount
                }
              >
                Create expense
              </button>
            </form>
          </section>

          <section className="card stack">
            <h2>Add Payment</h2>

            <form onSubmit={onCreatePayment} className="stack">
              <label>
                <span className="muted" style={{ fontSize: 12 }}>
                  From
                </span>
                <select
                  value={fromUserId}
                  onChange={(e) => setFromUserId(e.target.value)}
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

              <label>
                <span className="muted" style={{ fontSize: 12 }}>
                  To
                </span>
                <select
                  value={toUserId}
                  onChange={(e) => setToUserId(e.target.value)}
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
                min={0}
                required
              />

              <label>
                <span className="muted" style={{ fontSize: 12 }}>
                  Payment date
                </span>
                <input
                  type="date"
                  value={paymentDate}
                  onChange={(e) => setPaymentDate(e.target.value)}
                  required
                />
              </label>

              <button
                type="submit"
                disabled={!paymentAmount || knownUserIds.length === 0}
              >
                Create payment
              </button>
            </form>
          </section>

          <section className="card stack">
            <h2>Balances</h2>

            {data.balances.netBalances.length === 0 ? (
              <div className="muted">No balances yet.</div>
            ) : (
              <ul>
                {data.balances.netBalances.map((b) => (
                  <li key={b.userId}>
                    <b>{labelOf(b.userId)}</b>{" "}
                    <span
                      className="muted"
                      style={{ fontFamily: "monospace", fontSize: 12 }}
                    >
                      ({shortId(b.userId)})
                    </span>{" "}
                    :{" "}
                    <b style={{ color: b.amount >= 0 ? "green" : "crimson" }}>
                      {b.amount.toFixed(2)}
                    </b>
                  </li>
                ))}
              </ul>
            )}

            <h3>Suggested Transfers</h3>
            {data.balances.transfers.length === 0 ? (
              <p className="muted">No transfers needed.</p>
            ) : (
              <ul>
                {data.balances.transfers.map((t, idx) => (
                  <li key={idx}>
                    <b>{labelOf(t.fromUserId)}</b> → <b>{labelOf(t.toUserId)}</b>{" "}
                    : <b>{t.amount.toFixed(2)}</b>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="card stack">
            <h2>Expenses</h2>

            {data.expenses.length === 0 ? (
              <div className="muted">No expenses yet.</div>
            ) : (
              <ul>
                {data.expenses.map((e) => (
                  <li key={e.id}>
                    <b>{e.description}</b> — {e.amount.toFixed(2)} — paid by{" "}
                    <b>{labelOf(e.paidByUserId)}</b> —{" "}
                    <span className="muted">{e.expenseDate}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="card stack">
            <h2>Payments</h2>

            {data.payments.length === 0 ? (
              <div className="muted">No payments yet.</div>
            ) : (
              <ul>
                {data.payments.map((p) => (
                  <li key={p.id}>
                    <b>{labelOf(p.fromUserId)}</b> → <b>{labelOf(p.toUserId)}</b>{" "}
                    — {p.amount.toFixed(2)} —{" "}
                    <span className="muted">{p.paymentDate}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </div>
  );
}
