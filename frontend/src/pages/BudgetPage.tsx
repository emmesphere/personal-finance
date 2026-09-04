import { useState, type FormEvent } from "react";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import { useAuth } from "../auth/AuthContext";
import { setMonthlyBudget } from "../api/budgets";
import { getErrorMessage } from "../api/client";
import ErrorAlert from "../components/ErrorAlert";

const now = new Date();

export default function BudgetPage() {
  const { ledgerId } = useAuth();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [amount, setAmount] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!ledgerId) return;
    setError(null);
    setSuccess(false);
    setIsSubmitting(true);
    try {
      await setMonthlyBudget(ledgerId, year, month, { amount: Number(amount) });
      setSuccess(true);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="d-flex justify-content-center">
      <Card style={{ width: "24rem" }}>
        <Card.Body>
          <Card.Title className="mb-3">Set Monthly Budget</Card.Title>
          <ErrorAlert message={error} />
          {success && <div className="alert alert-success">Budget saved.</div>}
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3">
              <Form.Label>Year</Form.Label>
              <Form.Control
                type="number"
                value={year}
                onChange={(e) => setYear(Number(e.target.value))}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Month</Form.Label>
              <Form.Select value={month} onChange={(e) => setMonth(Number(e.target.value))}>
                {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </Form.Select>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Amount</Form.Label>
              <Form.Control
                type="number"
                min={0.01}
                step="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                required
              />
            </Form.Group>
            <Button type="submit" disabled={isSubmitting} className="w-100">
              {isSubmitting ? "Saving..." : "Save Budget"}
            </Button>
          </Form>
        </Card.Body>
      </Card>
    </div>
  );
}
