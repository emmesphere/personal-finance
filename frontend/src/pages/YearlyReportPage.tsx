import { useEffect, useState } from "react";
import Table from "react-bootstrap/Table";
import Form from "react-bootstrap/Form";
import { useAuth } from "../auth/AuthContext";
import { getYearlySummary } from "../api/reports";
import { getErrorMessage } from "../api/client";
import type { YearlySummaryReport } from "../types/api";
import LoadingSpinner from "../components/LoadingSpinner";
import ErrorAlert from "../components/ErrorAlert";

const MONTH_NAMES = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

export default function YearlyReportPage() {
  const { ledgerId } = useAuth();
  const [year, setYear] = useState(new Date().getFullYear());
  const [report, setReport] = useState<YearlySummaryReport | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!ledgerId) return;
    setIsLoading(true);
    getYearlySummary(ledgerId, year)
      .then(setReport)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setIsLoading(false));
  }, [ledgerId, year]);

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>Yearly Report</h1>
        <Form.Control
          type="number"
          style={{ width: "10rem" }}
          value={year}
          onChange={(e) => setYear(Number(e.target.value))}
        />
      </div>
      <ErrorAlert message={error} />
      {isLoading ? (
        <LoadingSpinner />
      ) : (
        <Table striped bordered hover>
          <thead>
            <tr>
              <th>Month</th>
              <th>Expenses</th>
            </tr>
          </thead>
          <tbody>
            {report?.months.map((m) => (
              <tr key={m.month}>
                <td>{MONTH_NAMES[m.month - 1]}</td>
                <td>{m.amount.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}
