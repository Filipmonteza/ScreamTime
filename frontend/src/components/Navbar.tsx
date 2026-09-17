import { useEffect, useRef, useState } from "react";
import { NavLink, Link, useLocation, useNavigate } from "react-router-dom";
import "./Navbar.css";
import { useAuth } from "../context/useAuth";
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
import iconLoginDefault from "../assets/icons/Icon_Login_Default.svg";
import iconLogoutDefault from "../assets/icons/Icon_Logout_Default.svg";

export type Role = "Admin" | "Staff" | "Customer" | null;

type NavItem = {
  to: string;
  label: string;
  iconDefault: string;
  iconFill: string;
  isLogout?: boolean;
};

type NavDropdownItem = {
  label: string;
  iconDefault: string;
  iconFill: string;
  children: NavItem[];
};

type NavEntry = NavItem | NavDropdownItem;

function isDropdown(entry: NavEntry): entry is NavDropdownItem {
  return "children" in entry;
}

const customerItems: NavEntry[] = [
  {
    to: "/",
    label: "Hem",
    iconDefault: iconHomeDefault,
    iconFill: iconHomeFill,
  },
  {
    to: "/all-movies",
    label: "Alla filmer",
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
    to: "/bookings",
    label: "Mina Bokningar",
    iconDefault: iconBookingsDefault,
    iconFill: iconBookingsFill,
  },
  {
    label: "Konto",
    iconDefault: iconProfileDefault,
    iconFill: iconProfileFill,
    children: [
      {
        to: "/account",
        label: "Mitt konto",
        iconDefault: iconProfileDefault,
        iconFill: iconProfileFill,
      },
      {
        to: "/login",
        label: "Logga ut",
        iconDefault: iconLogoutDefault,
        iconFill: iconLogoutDefault,
        isLogout: true,
      },
    ],
  },
];

const adminItems: NavEntry[] = [
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
    to: "/adminbookings",
    label: "Bokningar",
    iconDefault: iconBookingsDefault,
    iconFill: iconBookingsFill,
  },
  {
    to: "/auditoriums",
    label: "Salonger",
    iconDefault: iconAuditoriumsDefault,
    iconFill: iconAuditoriumsFill,
  },
  {
    label: "Konton",
    iconDefault: iconStaffDefault,
    iconFill: iconStaffFill,
    children: [
      {
        to: "/staff",
        label: "Personal",
        iconDefault: iconStaffDefault,
        iconFill: iconStaffFill,
      },
      {
        to: "/account",
        label: "Mitt konto",
        iconDefault: iconProfileDefault,
        iconFill: iconProfileFill,
      },
      {
        to: "/login",
        label: "Logga ut",
        iconDefault: iconLogoutDefault,
        iconFill: iconLogoutDefault,
        isLogout: true,
      },
    ],
  },
];

const loggedOutItems: NavItem[] = [
  {
    to: "/",
    label: "Hem",
    iconDefault: iconHomeDefault,
    iconFill: iconHomeFill,
  },
  {
    to: "/all-movies",
    label: "Alla filmer",
    iconDefault: iconMoviesDefault,
    iconFill: iconMoviesFill,
  },
  {
  to: "/screenings",
  label: "Visningar",
  iconDefault: iconScreeningsDefault,
  iconFill: iconScreeningsFill,
  },
];

function getNavItems(role: Role): NavEntry[] {
  if (role === "Admin") return adminItems;
  if (role === "Staff") {
    // Staff should see most admin items but not the staff management page
    return adminItems.map((entry) => {
      if (isDropdown(entry)) {
        return {
          ...entry,
          children: entry.children.filter((child) => child.to !== "/staff"),
        };
      }
      return entry;
    });
  }
  if (role === "Customer") return customerItems;
  return loggedOutItems;
}

type NavbarProps = {
  role: Role;
};

function Navbar({ role }: NavbarProps) {
  const items = getNavItems(role);
  const location = useLocation();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  const [prevPathname, setPrevPathname] = useState(location.pathname);
  if (location.pathname !== prevPathname) {
    setPrevPathname(location.pathname);
    setIsDropdownOpen(false);
  }

  useEffect(() => {
    if (!isDropdownOpen) return;
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsDropdownOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isDropdownOpen]);

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-logo">
        ScreenTime
      </Link>
      <div
        className={`navbar-links navbar-links--${role === "Customer" ? "customer" : "admin"}`}
      >
        {items.map((item) => {
          if (isDropdown(item)) {
            const isChildActive = item.children.some(
              (child) => location.pathname === child.to,
            );
            return (
              <div
                className="navbar-dropdown"
                ref={dropdownRef}
                key={item.label}
              >
                <button
                  type="button"
                  className={
                    isChildActive ? "navbar-link active" : "navbar-link"
                  }
                  onClick={() => setIsDropdownOpen((open) => !open)}
                >
                  <img
                    src={isChildActive ? item.iconFill : item.iconDefault}
                    alt=""
                    className="navbar-icon"
                  />
                  <span>{item.label}</span>
                </button>
                {isDropdownOpen && (
                  <div className="navbar-dropdown-menu">
                    {item.children.map((child) =>
                      child.isLogout ? (
                        <button
                          key={child.label}
                          type="button"
                          className="navbar-dropdown-item"
                          onClick={handleLogout}
                        >
                          <img
                            src={child.iconDefault}
                            alt=""
                            className="navbar-icon"
                          />
                          <span>{child.label}</span>
                        </button>
                      ) : (
                        <NavLink
                          key={child.to}
                          to={child.to}
                          className={({ isActive }) =>
                            isActive
                              ? "navbar-dropdown-item active"
                              : "navbar-dropdown-item"
                          }
                        >
                          {({ isActive }) => (
                            <>
                              <img
                                src={
                                  isActive ? child.iconFill : child.iconDefault
                                }
                                alt=""
                                className="navbar-icon"
                              />
                              <span>{child.label}</span>
                            </>
                          )}
                        </NavLink>
                      ),
                    )}
                  </div>
                )}
              </div>
            );
          }

          return (
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
          );
        })}
        {role === null && (
          <Link to="/login" className="navbar-link">
            <img src={iconLoginDefault} alt="" className="navbar-icon" />
            <span>Logga in</span>
          </Link>
        )}
      </div>
    </nav>
  );
}

export default Navbar;
