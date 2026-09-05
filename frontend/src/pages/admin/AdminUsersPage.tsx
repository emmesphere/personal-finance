import { useEffect, useState } from "react";
import Table from "react-bootstrap/Table";
import Button from "react-bootstrap/Button";
import Badge from "react-bootstrap/Badge";
import { deactivateUser, demoteUser, listUsers, promoteUser } from "../../api/admin";
import { getErrorMessage } from "../../api/client";
import type { AdminUser } from "../../types/api";
import LoadingSpinner from "../../components/LoadingSpinner";
import ErrorAlert from "../../components/ErrorAlert";

export default function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingUserId, setPendingUserId] = useState<string | null>(null);

  function loadUsers() {
    setIsLoading(true);
    listUsers()
      .then(setUsers)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }

  useEffect(loadUsers, []);

  async function runAction(userId: string, action: (id: string) => Promise<void>) {
    setError(null);
    setPendingUserId(userId);
    try {
      await action(userId);
      loadUsers();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setPendingUserId(null);
    }
  }

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <h1 className="mb-4">Admin: Users</h1>
      <ErrorAlert message={error} />
      <Table striped bordered hover>
        <thead>
          <tr>
            <th>Full name</th>
            <th>Username</th>
            <th>Role</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => {
            const isPending = pendingUserId === user.id;
            return (
              <tr key={user.id}>
                <td>{user.fullName}</td>
                <td>{user.username}</td>
                <td>{user.role}</td>
                <td>
                  <Badge bg={user.isActive ? "success" : "secondary"}>
                    {user.isActive ? "Active" : "Inactive"}
                  </Badge>
                </td>
                <td className="d-flex gap-2">
                  {user.role === "User" ? (
                    <Button
                      size="sm"
                      variant="outline-primary"
                      disabled={isPending}
                      onClick={() => runAction(user.id, promoteUser)}
                    >
                      Promote
                    </Button>
                  ) : (
                    <Button
                      size="sm"
                      variant="outline-secondary"
                      disabled={isPending}
                      onClick={() => runAction(user.id, demoteUser)}
                    >
                      Demote
                    </Button>
                  )}
                  <Button
                    size="sm"
                    variant="outline-danger"
                    disabled={isPending || !user.isActive}
                    onClick={() => runAction(user.id, deactivateUser)}
                  >
                    Deactivate
                  </Button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </Table>
    </div>
  );
}
