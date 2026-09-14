import type { ReactNode } from "react";
import "./ListCard.css";

interface ListCardProps {
  title: string;
  infoLines: ReactNode[];
  confirming: boolean;
  deleting?: boolean;
  deleteLabel?: string;
  statusLabel?: string;
  inactiveStyle?: boolean;
  showActions?: boolean;
  onEdit: () => void;
  onRequestDelete: () => void;
  onConfirmDelete: () => void;
  onCancelDelete: () => void;
  isActive?: boolean;
  confirmingReactivate?: boolean;
  reactivating?: boolean;
  onRequestReactivate?: () => void;
  onConfirmReactivate?: () => void;
  onCancelReactivate?: () => void;
}

function ListCard({
  title,
  infoLines,
  confirming,
  deleting,
  deleteLabel = "Ta bort",
  statusLabel,
  inactiveStyle = false,
  showActions = true,
  onEdit,
  onRequestDelete,
  onConfirmDelete,
  onCancelDelete,
  isActive = true,
  confirmingReactivate,
  reactivating,
  onRequestReactivate,
  onConfirmReactivate,
  onCancelReactivate,
}: ListCardProps) {
  const cardClassName = inactiveStyle
    ? "list-card list-card--inactive"
    : "list-card";

  return (
    <div className={cardClassName}>
      <div className="list-card-info">
        <h2>{title}</h2>

        {statusLabel && (
          <span className="list-card-status">{statusLabel}</span>
        )}

        {infoLines.map((line, i) => (
          <p key={i}>{line}</p>
        ))}
      </div>

      {showActions && (
        <>
          {confirming || confirmingReactivate ? (
            <div className="list-card-actions">
              <span>Är du säker?</span>

              <button
                className="btn-pill"
                onClick={
                  isActive ? onConfirmDelete : onConfirmReactivate
                }
                disabled={isActive ? deleting : reactivating}
              >
                Ja
              </button>

              <button
                className="btn-pill"
                onClick={
                  isActive ? onCancelDelete : onCancelReactivate
                }
              >
                Nej
              </button>
            </div>
          ) : (
            <div className="list-card-actions">
              <button
                className="btn-pill"
                onClick={onEdit}
                disabled={!isActive}
              >
                Redigera
              </button>

              <button
                className="btn-pill"
                onClick={
                  isActive ? onRequestDelete : onRequestReactivate
                }
              >
                {isActive ? deleteLabel : "Aktivera"}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default ListCard;