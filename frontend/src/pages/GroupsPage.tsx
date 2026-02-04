import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { http } from "../api/client";
import type { CreateGroupRequest, GroupResponse } from "../types/api";

export default function GroupsPage() {
  const [groups, setGroups] = useState<GroupResponse[]>([]);
  const [name, setName] = useState("");
  const [currency, setCurrency] = useState("DKK");

  const [isLoading, setIsLoading] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    setIsLoading(true);
    try {
      const data = await http<GroupResponse[]>("/api/groups");
      setGroups(data);
    } catch (e: any) {
      setError(e?.message ?? "Failed to load groups");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    const body: CreateGroupRequest = { name: name.trim(), currency: currency.trim() || "DKK" };

    setIsCreating(true);
    try {
      await http("/api/groups", { method: "POST", body: JSON.stringify(body) });
      setName("");
      await load();
    } catch (e: any) {
      setError(e?.message ?? "Failed to create group");
    } finally {
      setIsCreating(false);
    }
  }

  return (
    <div className="stack">
      <section className="card stack">
        <h1>Groups</h1>
        <p className="muted">Create a group, then add expenses and payments.</p>

        <form onSubmit={onCreate} className="stack">
          <div className="stack" style={{ gap: 8 }}>
            <input
              placeholder="Group name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <input
              placeholder="Currency (e.g. DKK)"
              value={currency}
              onChange={(e) => setCurrency(e.target.value)}
            />
          </div>

          <button type="submit" disabled={isCreating || !name.trim()}>
            {isCreating ? "Creating…" : "Create group"}
          </button>
        </form>

        {error && <div className="errorBox">{error}</div>}
      </section>

      <section className="card stack">
        <h2>All groups</h2>

        {isLoading ? (
          <div className="muted">Loading…</div>
        ) : groups.length === 0 ? (
          <div className="muted">No groups yet. Create one above.</div>
        ) : (
          <ul>
            {groups.map((g) => (
              <li key={g.id}>
                <Link to={`/groups/${g.id}`}>
                  <b>{g.name}</b>
                </Link>{" "}
                <span className="muted">({g.currency})</span>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
