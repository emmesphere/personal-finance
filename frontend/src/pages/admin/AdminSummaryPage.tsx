import { useEffect, useState } from "react";
import Row from "react-bootstrap/Row";
import Col from "react-bootstrap/Col";
import Card from "react-bootstrap/Card";
import { getAdminSummary } from "../../api/admin";
import { getErrorMessage } from "../../api/client";
import type { AdminSummary } from "../../types/api";
import LoadingSpinner from "../../components/LoadingSpinner";
import ErrorAlert from "../../components/ErrorAlert";

export default function AdminSummaryPage() {
  const [summary, setSummary] = useState<AdminSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getAdminSummary()
      .then(setSummary)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }, []);

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <h1 className="mb-4">Admin: System Summary</h1>
      <ErrorAlert message={error} />
      {summary && (
        <Row>
          <Col md={4}>
            <Card>
              <Card.Body>
                <Card.Subtitle className="text-muted mb-2">Total Users</Card.Subtitle>
                <Card.Title>{summary.totalUsers}</Card.Title>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card>
              <Card.Body>
                <Card.Subtitle className="text-muted mb-2">Total Ledgers</Card.Subtitle>
                <Card.Title>{summary.totalLedgers}</Card.Title>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card>
              <Card.Body>
                <Card.Subtitle className="text-muted mb-2">Journal Entries This Month</Card.Subtitle>
                <Card.Title>{summary.postedJournalEntriesThisMonth}</Card.Title>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  );
}
