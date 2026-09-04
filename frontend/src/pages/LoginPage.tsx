import { useState, type FormEvent } from "react";
import { Navigate, Link } from "react-router-dom";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import ErrorAlert from "../components/ErrorAlert";

export default function LoginPage() {
  const { login, token } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (token) return <Navigate to="/" replace />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await login({ username, password });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="d-flex justify-content-center mt-5">
      <Card style={{ width: "24rem" }}>
        <Card.Body>
          <Card.Title className="mb-3">Log in</Card.Title>
          <ErrorAlert message={error} />
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3">
              <Form.Label>Username</Form.Label>
              <Form.Control
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Password</Form.Label>
              <Form.Control
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </Form.Group>
            <Button type="submit" disabled={isSubmitting} className="w-100">
              {isSubmitting ? "Logging in..." : "Log in"}
            </Button>
          </Form>
          <div className="mt-3 text-center">
            <Link to="/register">Need an account? Register</Link>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
}
