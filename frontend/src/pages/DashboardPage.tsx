import { useEffect, useState } from "react";
import Card from "react-bootstrap/Card";
import Row from "react-bootstrap/Row";
import Col from "react-bootstrap/Col";
import Table from "react-bootstrap/Table";
import { useAuth } from "../auth/AuthContext";
import { getDashboard } from "../api/reports";
import { getErrorMessage } from "../api/client";
import type { DashboardReport } from "../types/api";
import LoadingSpinner from "../components/LoadingSpinner";
import ErrorAlert from "../components/ErrorAlert";

const now = new Date();

export default function DashboardPage() {
  const { ledgerId } = useAuth();
  const [report, setReport] = useState<DashboardReport | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!ledgerId) return;
    setIsLoading(true);
    getDashboard(ledgerId, now.getFullYear(), now.getMonth() + 1)
      .then(setReport)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }, [ledgerId]);

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <h1 className="mb-4">Dashboard</h1>
      <ErrorAlert message={error} />
      {report && (
        <>
          <Row className="mb-4">
            <Col md={4}>
              <Card>
                <Card.Body>
                  <Card.Subtitle className="text-muted mb-2">Total Balance</Card.Subtitle>
                  <Card.Title>{report.totalBalance.toFixed(2)}</Card.Title>
                </Card.Body>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <Card.Body>
                  <Card.Subtitle className="text-muted mb-2">Expenses This Month</Card.Subtitle>
                  <Card.Title>{report.totalExpenses.toFixed(2)}</Card.Title>
                </Card.Body>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <Card.Body>
                  <Card.Subtitle className="text-muted mb-2">Budget</Card.Subtitle>
                  <Card.Title>
                    {report.budgetAmount != null ? report.budgetAmount.toFixed(2) : "Not set"}
                  </Card.Title>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          <h2 className="h4 mb-3">Expenses by Category</h2>
          <Table striped bordered hover>
            <thead>
              <tr>
                <th>Category</th>
                <th>Amount</th>
              </tr>
            </thead>
            <tbody>
              {report.expensesByCategory.length === 0 && (
                <tr>
                  <td colSpan={2} className="text-center text-muted">
                    No expenses recorded this month.
                  </td>
                </tr>
              )}
              {report.expensesByCategory.map((item) => (
                <tr key={item.categoryId}>
                  <td>{item.categoryName}</td>
                  <td>{item.amount.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        </>
      )}
    </div>
  );
}
