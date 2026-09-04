import BsNavbar from "react-bootstrap/Navbar";
import Nav from "react-bootstrap/Nav";
import Container from "react-bootstrap/Container";
import { NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function Navbar() {
  const { token, isAdmin, me, logout } = useAuth();

  if (!token) return null;

  return (
    <BsNavbar bg="primary" data-bs-theme="dark" expand="lg" className="mb-4">
      <Container>
        <BsNavbar.Brand as={NavLink} to="/">
          Personal Finance
        </BsNavbar.Brand>
        <BsNavbar.Toggle aria-controls="main-navbar" />
        <BsNavbar.Collapse id="main-navbar">
          <Nav className="me-auto">
            <Nav.Link as={NavLink} to="/">Dashboard</Nav.Link>
            <Nav.Link as={NavLink} to="/accounts">Accounts</Nav.Link>
            <Nav.Link as={NavLink} to="/categories">Categories</Nav.Link>
            <Nav.Link as={NavLink} to="/income/new">Record Income</Nav.Link>
            <Nav.Link as={NavLink} to="/expenses/new">Record Expense</Nav.Link>
            <Nav.Link as={NavLink} to="/budget">Budget</Nav.Link>
            <Nav.Link as={NavLink} to="/reports/yearly">Yearly Report</Nav.Link>
            {isAdmin && (
              <>
                <Nav.Link as={NavLink} to="/admin/users">Admin: Users</Nav.Link>
                <Nav.Link as={NavLink} to="/admin/summary">Admin: Summary</Nav.Link>
              </>
            )}
          </Nav>
          <Nav>
            <BsNavbar.Text className="me-3">{me?.username}</BsNavbar.Text>
            <Nav.Link onClick={logout}>Log out</Nav.Link>
          </Nav>
        </BsNavbar.Collapse>
      </Container>
    </BsNavbar>
  );
}
