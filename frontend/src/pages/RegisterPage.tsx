import { useState, type FormEvent } from "react";
import { Navigate, Link } from "react-router-dom";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import { useAuth } from "../auth/AuthContext";
import { getErrorMessage } from "../api/client";
import ErrorAlert from "../components/ErrorAlert";

export default function RegisterPage() {
  const { register, token } = useAuth();
  const [fullName, setFullName] = useState("");
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (token) return <Navigate to="/" replace />;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    if (!email && !phoneNumber) {
      setError("Provide either an email or a phone number.");
      return;
    }

    setIsSubmitting(true);
    try {
      await register({
        fullName,
        username,
        email: email || undefined,
        phoneNumber: phoneNumber || undefined,
        password,
      });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="d-flex justify-content-center mt-5">
      <Card style={{ width: "26rem" }}>
        <Card.Body>
          <Card.Title className="mb-3">Create an account</Card.Title>
          <ErrorAlert message={error} />
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3">
              <Form.Label>Full name</Form.Label>
              <Form.Control
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Username</Form.Label>
              <Form.Control
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Email</Form.Label>
              <Form.Control
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Phone number</Form.Label>
              <Form.Control
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
              />
              <Form.Text muted>Provide at least an email or a phone number.</Form.Text>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Password</Form.Label>
              <Form.Control
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                minLength={8}
                required
              />
              <Form.Text muted>At least 8 characters.</Form.Text>
            </Form.Group>
            <Button type="submit" disabled={isSubmitting} className="w-100">
              {isSubmitting ? "Creating account..." : "Register"}
            </Button>
          </Form>
          <div className="mt-3 text-center">
            <Link to="/login">Already have an account? Log in</Link>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
}
