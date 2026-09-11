import type { ReactNode } from "react";
import "./ListCard.css";

interface ListCardProps {
  title: string;
  infoLines: ReactNode[];
  confirming: boolean;
  deleting?: boolean;
  onEdit: () => void;
  onRequestDelete: () => void;
  onConfirmDelete: () => void;
  onCancelDelete: () => void;
}

function ListCard({
  title,
  infoLines,
  confirming,
  deleting,
  onEdit,
  onRequestDelete,
  onConfirmDelete,
  onCancelDelete,
}: ListCardProps) {
  return (
    <div className="list-card">
      <div className="list-card-info">
        <h2>{title}</h2>
        {infoLines.map((line, i) => (
          <p key={i}>{line}</p>
        ))}
      </div>

      {confirming ? (
        <div className="list-card-actions">
          <span>Är du säker?</span>
          <button
            className="btn-pill"
            onClick={onConfirmDelete}
            disabled={deleting}
          >
            Ja
          </button>
          <button className="btn-pill" onClick={onCancelDelete}>
            Nej
          </button>
        </div>
      ) : (
        <div className="list-card-actions">
          <button className="btn-pill" onClick={onEdit}>
            Redigera
          </button>
          <button className="btn-pill" onClick={onRequestDelete}>
            Ta bort
          </button>
        </div>
      )}
    </div>
  );
}

export default ListCard;
