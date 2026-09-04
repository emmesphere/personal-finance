import { useEffect, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import Form from "react-bootstrap/Form";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import { useAuth } from "../auth/AuthContext";
import { listAccounts } from "../api/accounts";
import { listCategories } from "../api/categories";
import { addIncome } from "../api/incomes";
import { getErrorMessage } from "../api/client";
import type { Account, Category } from "../types/api";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";

const RECEIVING_ACCOUNT_TYPES = ["BankAccount", "Wallet", "Benefit", "Debit"];

function todayIso() {
  return new Date().toISOString().slice(0, 10);
}

export default function RecordIncomePage() {
  const { ledgerId } = useAuth();
  const navigate = useNavigate();

  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [categoryId, setCategoryId] = useState("");
  const [receivingAccountId, setReceivingAccountId] = useState("");
  const [amount, setAmount] = useState("");
  const [date, setDate] = useState(todayIso());
  const [description, setDescription] = useState("");
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    if (!ledgerId) return;
    Promise.all([listAccounts(ledgerId), listCategories("Income")])
      .then(([accountsResult, categoriesResult]) => {
        setAccounts(accountsResult.filter((a) => RECEIVING_ACCOUNT_TYPES.includes(a.type)));
        setCategories(categoriesResult);
      })
      .catch((err) => setLoadError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }, [ledgerId]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!ledgerId) return;
    setFormError(null);
    setIsSubmitting(true);
    try {
      await addIncome(ledgerId, {
        categoryId,
        receivingAccountId,
        amount: Number(amount),
        date,
        description: description || undefined,
      });
      setSuccess(true);
      setTimeout(() => navigate("/"), 800);
    } catch (err) {
      setFormError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) return <LoadingSpinner />;

  return (
    <div className="d-flex justify-content-center">
      <Card style={{ width: "28rem" }}>
        <Card.Body>
          <Card.Title className="mb-3">Record Income</Card.Title>
          <ErrorAlert message={loadError} />
          <ErrorAlert message={formError} />
          {success && <div className="alert alert-success">Income recorded.</div>}
          {accounts.length === 0 || categories.length === 0 ? (
            <p className="text-muted">
              You need at least one income category and one bank/wallet/benefit/debit account
              before recording an income.
            </p>
          ) : (
            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-3">
                <Form.Label>Category</Form.Label>
                <Form.Select
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  required
                >
                  <option value="">Select a category</option>
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </Form.Select>
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Receiving account</Form.Label>
                <Form.Select
                  value={receivingAccountId}
                  onChange={(e) => setReceivingAccountId(e.target.value)}
                  required
                >
                  <option value="">Select an account</option>
                  {accounts.map((a) => (
                    <option key={a.id} value={a.id}>{a.name}</option>
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
              <Form.Group className="mb-3">
                <Form.Label>Date</Form.Label>
                <Form.Control
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  required
                />
              </Form.Group>
              <Form.Group className="mb-3">
                <Form.Label>Description (optional)</Form.Label>
                <Form.Control value={description} onChange={(e) => setDescription(e.target.value)} />
              </Form.Group>
              <Button type="submit" disabled={isSubmitting} className="w-100">
                {isSubmitting ? "Recording..." : "Record Income"}
              </Button>
            </Form>
          )}
        </Card.Body>
      </Card>
    </div>
  );
}
