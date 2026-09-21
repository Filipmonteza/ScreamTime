import { useEffect, useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Auditorium } from "../../types/auditorium";
import type { SeatType } from "../../types/seatType";
import Message from "../../components/Message";
import {
  createAuditorium,
  updateAuditorium,
  addSeatsToAuditorium,
} from "../../api/auditoriumsApi";
import { getAllSeatTypes } from "../../api/seatsApi";

function buildSeatRows(
  rowCount: number,
  seatsPerRow: number,
  seatTypeId: string,
) {
  const rows = [];
  for (let row = 0; row < rowCount; row++) {
    for (let seatNumber = 1; seatNumber <= seatsPerRow; seatNumber++) {
      rows.push({
        rowLabel: String.fromCharCode(65 + row),
        seatNumber,
        seatTypeId,
      });
    }
  }
  return rows;
}

type AuditoriumFormProps = {
  auditorium?: Auditorium;
  onClose: () => void;
  onSaved: () => void;
  // Reload the list without closing the form, so a failure after the
  // auditorium was already created can still be shown to the user.
  onRefresh: () => void;
};

function AuditoriumForm({
  auditorium,
  onClose,
  onSaved,
  onRefresh,
}: AuditoriumFormProps) {
  const [name, setName] = useState(auditorium?.name ?? "");
  const [isActive, setIsActive] = useState(auditorium?.isActive ?? true);
  const [rowCount, setRowCount] = useState("");
  const [seatsPerRow, setSeatsPerRow] = useState("");
  const [seatTypes, setSeatTypes] = useState<SeatType[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getAllSeatTypes().then(setSeatTypes);
  }, []);

  const standardSeatTypeId = seatTypes.find((st) => st.name === "Standard")?.id;

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    if (auditorium) {
      updateAuditorium(auditorium.auditoriumId, { name, isActive })
        .then(onSaved)
        .catch((err) =>
          setError(err instanceof Error ? err.message : "Något gick fel."),
        );
      return;
    }
    createAuditorium({ name })
      .then((created) => {
        const rows = Number(rowCount);
        const seats = Number(seatsPerRow);
        if (rows > 0 && seats > 0 && standardSeatTypeId) {
          addSeatsToAuditorium(
            created.auditoriumId,
            buildSeatRows(rows, seats, standardSeatTypeId),
          )
            .then(onSaved)
            .catch((err) => {
              // The auditorium exists even though the seats failed: refresh the
              // list behind the form, but keep the form up to carry the error.
              onRefresh();
              // Some messages come from the API already punctuated, others are
              // built from a status code, so normalise before joining them.
              const reason = err instanceof Error ? err.message.trim() : "";
              const detail = reason
                ? ` ${reason.endsWith(".") ? reason : `${reason}.`}`
                : "";
              setError(
                `Salongen skapades men sätena kunde inte läggas till.${detail}` +
                  " Ta bort salongen och försök igen.",
              );
            });
        } else {
          onSaved();
        }
      })
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Något gick fel."),
      );
  }

  return (
    <Form
      title={auditorium ? "Redigera salong" : "Lägg till salong"}
      submitLabel={auditorium ? "Spara ändringar" : "Spara salong"}
      onSubmit={handleSubmit}
      onCancel={onClose}
    >
      <div className="form-field">
        <label className="form-label">Namn</label>
        <input
          className="form-input"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
      </div>
      {error && <Message type="error">{error}</Message>}
      {!auditorium && (
        <div className="form-field">
          <label className="form-label">Antal rader</label>
          <input
            className="form-input"
            type="number"
            min={0}
            value={rowCount}
            onChange={(e) => setRowCount(e.target.value)}
          />
        </div>
      )}
      {!auditorium && (
        <div className="form-field">
          <label className="form-label">Säten per rad</label>
          <input
            className="form-input"
            type="number"
            min={0}
            value={seatsPerRow}
            onChange={(e) => setSeatsPerRow(e.target.value)}
          />
        </div>
      )}
      {auditorium && (
        <div className="form-field">
          <label className="form-label">
            <input
              type="checkbox"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
            />{" "}
            Aktiv
          </label>
        </div>
      )}
    </Form>
  );
}

export default AuditoriumForm;
