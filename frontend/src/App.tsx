import { Link, Route, Routes } from "react-router-dom";
import GroupsPage from "./pages/GroupsPage";
import GroupPage from "./pages/GroupPage";

export default function App() {
  return (
    <div>
      <header className="app__header">
        <div className="container" style={{ display: "flex", gap: 12, alignItems: "center" }}>
          <Link to="/" style={{ fontWeight: 800 }}>
            Roommate Expense Splitter
          </Link>
          <span className="muted" style={{ fontSize: 12 }}>
            v1
          </span>
        </div>
      </header>

      <main className="app__main">
        <div className="container">
          <Routes>
            <Route path="/" element={<GroupsPage />} />
            <Route path="/groups/:groupId" element={<GroupPage />} />
          </Routes>
        </div>
      </main>
    </div>
  );
}
