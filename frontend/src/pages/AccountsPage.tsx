import { useEffect, useState, type FormEvent } from "react";
import Table from "react-bootstrap/Table";
import Button from "react-bootstrap/Button";
import Modal from "react-bootstrap/Modal";
import Form from "react-bootstrap/Form";
import Badge from "react-bootstrap/Badge";
import { useAuth } from "../auth/AuthContext";
import { createAccount, listAccounts } from "../api/accounts";
import { getErrorMessage } from "../api/client";
import type { Account, AccountType } from "../types/api";
import LoadingSpinner from "../components/LoadingSpinner";
import ErrorAlert from "../components/ErrorAlert";

const ACCOUNT_TYPES: AccountType[] = ["BankAccount", "Wallet", "Benefit", "CreditCard", "Debit", "Loan"];
const DUE_DATE_TYPES: AccountType[] = ["CreditCard", "Debit", "Loan"];

export default function AccountsPage() {
  const { ledgerId } = useAuth();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);

  const [name, setName] = useState("");
  const [type, setType] = useState<AccountType>("BankAccount");
  const [dueDateDay, setDueDateDay] = useState("");
  const [openingBalance, setOpeningBalance] = useState("");
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function loadAccounts() {
    if (!ledgerId) return;
    setIsLoading(true);
    listAccounts(ledgerId)
      .then(setAccounts)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }

  useEffect(loadAccounts, [ledgerId]);

  function openModal() {
    setName("");
    setType("BankAccount");
    setDueDateDay("");
    setOpeningBalance("");
    setFormError(null);
    setShowModal(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!ledgerId) return;
    setFormError(null);
    setIsSubmitting(true);
    try {
      await createAccount(ledgerId, {
        name,
        type,
        dueDateDay: dueDateDay ? Number(dueDateDay) : undefined,
        openingBalance: openingBalance ? Number(openingBalance) : undefined,
      });
      setShowModal(false);
      loadAccounts();
    } catch (err) {
      setFormError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Accounts</h1>
        <Button onClick={openModal}>New Account</Button>
      </div>
      <ErrorAlert message={error} />
      {isLoading ? (
        <LoadingSpinner />
      ) : (
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Type</th>
              <th>Due day</th>
              <th>Balance</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {accounts.length === 0 && (
              <tr>
                <td colSpan={5} className="text-center text-muted">
                  No accounts yet.
                </td>
              </tr>
            )}
            {accounts.map((account) => (
              <tr key={account.id}>
                <td>{account.name}</td>
                <td>{account.type}</td>
                <td>{account.dueDateDay ?? "-"}</td>
                <td>{account.balance.toFixed(2)}</td>
                <td>
                  <Badge bg={account.isActive ? "success" : "secondary"}>
                    {account.isActive ? "Active" : "Inactive"}
                  </Badge>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}

      <Modal show={showModal} onHide={() => setShowModal(false)}>
        <Form onSubmit={handleSubmit}>
          <Modal.Header closeButton>
            <Modal.Title>New Account</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <ErrorAlert message={formError} />
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control value={name} onChange={(e) => setName(e.target.value)} required />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Type</Form.Label>
              <Form.Select value={type} onChange={(e) => setType(e.target.value as AccountType)}>
                {ACCOUNT_TYPES.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </Form.Select>
            </Form.Group>
            {DUE_DATE_TYPES.includes(type) && (
              <Form.Group className="mb-3">
                <Form.Label>Due date day (1-31)</Form.Label>
                <Form.Control
                  type="number"
                  min={1}
                  max={31}
                  value={dueDateDay}
                  onChange={(e) => setDueDateDay(e.target.value)}
                />
              </Form.Group>
            )}
            <Form.Group className="mb-3">
              <Form.Label>Opening balance (optional)</Form.Label>
              <Form.Control
                type="number"
                min={0}
                step="0.01"
                value={openingBalance}
                onChange={(e) => setOpeningBalance(e.target.value)}
              />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowModal(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create"}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </div>
  );
}
