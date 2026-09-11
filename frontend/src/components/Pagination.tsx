import iconChevronLeft from "../assets/icons/Icon_Chevron_Left.svg";
import iconChevronLeftDouble from "../assets/icons/Icon_Chevron_Left_Double.svg";
import iconChevronRight from "../assets/icons/Icon_Chevron_Right.svg";
import iconChevronRightDouble from "../assets/icons/Icon_Chevron_Right_Double.svg";
import "./Pagination.css";

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  let start = page - 1;
  let end = page + 1;
  if (start < 1) {
    end += 1 - start;
    start = 1;
  }
  if (end > totalPages) {
    start -= end - totalPages;
    end = totalPages;
  }
  start = Math.max(1, start);

  const pageNumbers: number[] = [];
  for (let n = start; n <= end; n++) pageNumbers.push(n);

  return (
    <div className="pagination">
      <button onClick={() => onPageChange(1)} disabled={page === 1}>
        <img src={iconChevronLeftDouble} alt="Första sidan" />
      </button>
      <button
        onClick={() => onPageChange(Math.max(1, page - 1))}
        disabled={page === 1}
      >
        <img src={iconChevronLeft} alt="Föregående sida" />
      </button>
      {pageNumbers.map((n) => (
        <button
          key={n}
          className={n === page ? "page-number active" : "page-number"}
          onClick={() => onPageChange(n)}
        >
          {n}
        </button>
      ))}
      <button
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
      >
        <img src={iconChevronRight} alt="Nästa sida" />
      </button>
      <button
        onClick={() => onPageChange(totalPages)}
        disabled={page === totalPages}
      >
        <img src={iconChevronRightDouble} alt="Sista sidan" />
      </button>
    </div>
  );
}

export default Pagination;
