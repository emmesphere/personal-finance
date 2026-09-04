import Alert from "react-bootstrap/Alert";

export default function ErrorAlert({ message }: { message: string | null }) {
  if (!message) return null;
  return <Alert variant="danger">{message}</Alert>;
}
