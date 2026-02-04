import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { http } from "../api/client";
import type { GroupResponse, CreateGroupRequest } from "../types/api";

export default function GroupsPage() {
  const [groups, setGroups] = useState<GroupResponse[]>([]);
  const [name, setName] = useState("");
  const [currency, setCurrency] = useState("DKK");
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    const data = await http<GroupResponse[]>("/api/groups");
    setGroups(data);
  }

  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    const body: CreateGroupRequest = { name, currency };
    await http("/api/groups", { method: "POST", body: JSON.stringify(body) });

    setName("");
    await load();
  }

  return (
    <div style={{ maxWidth: 900, margin: "40px auto", padding: 16 }}>
      <h1>Roommate Splitter</h1>

      <form onSubmit={onCreate} style={{ display: "flex", gap: 8, margin: "16px 0" }}>
        <input
          placeholder="Group name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          style={{ flex: 1, padding: 8 }}
        />
        <input
          placeholder="Currency"
          value={currency}
          onChange={(e) => setCurrency(e.target.value)}
          style={{ width: 120, padding: 8 }}
        />
        <button type="submit" style={{ padding: "8px 12px" }}>Create</button>
      </form>

      {error && <pre style={{ color: "crimson" }}>{error}</pre>}

      <ul>
        {groups.map((g) => (
          <li key={g.id}>
            <Link to={`/groups/${g.id}`}>{g.name}</Link> <small>({g.currency})</small>
          </li>
        ))}
      </ul>
    </div>
  );
}
