import { useEffect, useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Auditorium } from "../../types/auditorium";
import type { SeatType } from "../../types/seatType";
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
};

function AuditoriumForm({ auditorium, onClose, onSaved }: AuditoriumFormProps) {
  const [name, setName] = useState(auditorium?.name ?? "");
  const [isActive, setIsActive] = useState(auditorium?.isActive ?? true);
  const [rowCount, setRowCount] = useState("");
  const [seatsPerRow, setSeatsPerRow] = useState("");
  const [seatTypes, setSeatTypes] = useState<SeatType[]>([]);

  useEffect(() => {
    getAllSeatTypes().then(setSeatTypes);
  }, []);

  const standardSeatTypeId = seatTypes.find((st) => st.name === "Standard")?.id;

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (auditorium) {
      updateAuditorium(auditorium.auditoriumId, { name, isActive }).then(
        onSaved,
      );
      return;
    }
    createAuditorium({ name }).then((created) => {
      const rows = Number(rowCount);
      const seats = Number(seatsPerRow);
      if (rows > 0 && seats > 0 && standardSeatTypeId) {
        addSeatsToAuditorium(
          created.auditoriumId,
          buildSeatRows(rows, seats, standardSeatTypeId),
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
