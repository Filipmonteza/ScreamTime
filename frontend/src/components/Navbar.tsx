import { NavLink, Link } from "react-router-dom";
import "./Navbar.css";

import iconHomeDefault from "../assets/icons/Icon_Home_Default.svg";
import iconHomeFill from "../assets/icons/Icon_Home_Fill.svg";
import iconBookingsDefault from "../assets/icons/Icon_Bookings_Deafult.svg";
import iconBookingsFill from "../assets/icons/Icon_Bookings_Fill.svg";
import iconProfileDefault from "../assets/icons/Icon_Profile_Default.svg";
import iconProfileFill from "../assets/icons/Icon_Profile_Fill.svg";
import iconMoviesDefault from "../assets/icons/Icon_Movies_Default.svg";
import iconMoviesFill from "../assets/icons/Icon_Movies_Fill.svg";
import iconScreeningsDefault from "../assets/icons/Icon_Screenings_Default.svg";
import iconScreeningsFill from "../assets/icons/Icon_Screenings_Fill.svg";
import iconAuditoriumsDefault from "../assets/icons/Icon_Auditoriums_Default.svg";
import iconAuditoriumsFill from "../assets/icons/Icon_Auditoriums_Fill.svg";
import iconStaffDefault from "../assets/icons/Icon_Staff_Default.svg";
import iconStaffFill from "../assets/icons/Icon_Staff_Fill.svg";

export type Role = "Admin" | "Staff" | "Customer" | null;

type NavItem = {
  to: string;
  label: string;
  iconDefault: string;
  iconFill: string;
};

const customerItems: NavItem[] = [
  {
    to: "/",
    label: "Hem",
    iconDefault: iconHomeDefault,
    iconFill: iconHomeFill,
  },
  {
    to: "/bookings",
    label: "Bokningar",
    iconDefault: iconBookingsDefault,
    iconFill: iconBookingsFill,
  },
  {
    to: "/account",
    label: "Mitt konto",
    iconDefault: iconProfileDefault,
    iconFill: iconProfileFill,
  },
];

const adminItems: NavItem[] = [
  {
    to: "/",
    label: "Hem",
    iconDefault: iconHomeDefault,
    iconFill: iconHomeFill,
  },
  {
    to: "/movies",
    label: "Filmer",
    iconDefault: iconMoviesDefault,
    iconFill: iconMoviesFill,
  },
  {
    to: "/screenings",
    label: "Visningar",
    iconDefault: iconScreeningsDefault,
    iconFill: iconScreeningsFill,
  },
  {
    to: "/auditoriums",
    label: "Salonger",
    iconDefault: iconAuditoriumsDefault,
    iconFill: iconAuditoriumsFill,
  },
  {
    to: "/staff",
    label: "Personal",
    iconDefault: iconStaffDefault,
    iconFill: iconStaffFill,
  },
];

const loggedOutItems: NavItem[] = [
  {
    to: "/",
    label: "Hem",
    iconDefault: iconHomeDefault,
    iconFill: iconHomeFill,
  },
];

function getNavItems(role: Role): NavItem[] {
  if (role === "Admin" || role === "Staff") return adminItems;
  if (role === "Customer") return customerItems;
  return loggedOutItems;
}

type NavbarProps = {
  role: Role;
};

function Navbar({ role }: NavbarProps) {
  const items = getNavItems(role);

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-logo">
        ScreenTime
      </Link>
      <div className={`navbar-links navbar-links--${role === "Customer" ? "customer" : "admin"}`}>
        {items.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === "/"}
            className={({ isActive }) =>
              isActive ? "navbar-link active" : "navbar-link"
            }
          >
            {({ isActive }) => (
              <>
                <img
                  src={isActive ? item.iconFill : item.iconDefault}
                  alt=""
                  className="navbar-icon"
                />
                <span>{item.label}</span>
              </>
            )}
          </NavLink>
        ))}
        {role === null && (
          <Link to="/login" className="navbar-link">
            Logga in
          </Link>
        )}
      </div>
    </nav>
  );
}

export default Navbar;
