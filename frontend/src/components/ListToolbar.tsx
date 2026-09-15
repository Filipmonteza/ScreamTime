import Pagination from "./Pagination";
import "./ListToolbar.css";

interface ListToolbarProps {
  addLabel?: string;
  onAdd?: () => void;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

function ListToolbar({
  addLabel,
  onAdd,
  page,
  totalPages,
  onPageChange,
}: ListToolbarProps) {
  return (
    <div className="list-toolbar">
      {addLabel && onAdd && (
        <button className="btn-pill list-toolbar-add" onClick={onAdd}>
          {addLabel}
        </button>
      )}

      <Pagination
        page={page}
        totalPages={totalPages}
        onPageChange={onPageChange}
      />
    </div>
  );
}

export default ListToolbar;