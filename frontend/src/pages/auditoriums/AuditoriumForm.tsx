import { useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Auditorium } from "../../types/auditorium";
import {
  createAuditorium,
  updateAuditorium,
  addSeatsToAuditorium,
} from "../../api/auditoriumsApi";

// TODO: fetch när vi här sätestyper endpoints instället för att hårdkoda
const STANDARD_SEAT_TYPE_ID = "";

function buildSeatRows(rowCount: number, seatsPerRow: number) {
  const rows = [];
  for (let row = 0; row < rowCount; row++) {
    for (let seatNumber = 1; seatNumber <= seatsPerRow; seatNumber++) {
      rows.push({
        rowLabel: String.fromCharCode(65 + row),
        seatNumber,
        seatTypeId: STANDARD_SEAT_TYPE_ID,
      });
    }
  }
  return rows;
}

type AuditoriumFormProps = {
  auditorium?: Auditorium;
  onClose: () => void;
  onSaved: () => void;
};

function AuditoriumForm({ auditorium, onClose, onSaved }: AuditoriumFormProps) {
  const [name, setName] = useState(auditorium?.name ?? "");
  const [isActive, setIsActive] = useState(auditorium?.isActive ?? true);
  const [rowCount, setRowCount] = useState(0);
  const [seatsPerRow, setSeatsPerRow] = useState(0);

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (auditorium) {
      updateAuditorium(auditorium.auditoriumId, { name, isActive }).then(
        onSaved,
      );
      return;
    }
    createAuditorium({ name }).then((created) => {
      if (rowCount > 0 && seatsPerRow > 0) {
        addSeatsToAuditorium(
          created.auditoriumId,
          buildSeatRows(rowCount, seatsPerRow),
        ).then(onSaved);
      } else {
        onSaved();
      }
    });
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
      {!auditorium && (
        <div className="form-field">
          <label className="form-label">Antal rader</label>
          <input
            className="form-input"
            type="number"
            min={0}
            value={rowCount}
            onChange={(e) => setRowCount(Number(e.target.value))}
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
            onChange={(e) => setSeatsPerRow(Number(e.target.value))}
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
