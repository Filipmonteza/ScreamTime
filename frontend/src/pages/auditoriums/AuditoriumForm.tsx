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

const MAX_ROWS = 26; // A-Z; String.fromCharCode would silently produce
// non-letter characters (91+) past this, so it's a hard ceiling, not a UX nicety.

function buildSeatRows(
  rowCount: number,
  seatsPerRow: number,
  standardSeatTypeId: string,
  vipSeatTypeId: string | undefined,
  vipRowLabels: string[],
) {
  const rows = [];
  for (let row = 0; row < rowCount; row++) {
    const rowLabel = String.fromCharCode(65 + row);
    const isVipRow = Boolean(vipSeatTypeId) && vipRowLabels.includes(rowLabel);
    const seatTypeId = isVipRow ? (vipSeatTypeId as string) : standardSeatTypeId;
    for (let seatNumber = 1; seatNumber <= seatsPerRow; seatNumber++) {
      rows.push({
        rowLabel,
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
  const [vipRows, setVipRows] = useState("");
  const [seatTypes, setSeatTypes] = useState<SeatType[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getAllSeatTypes().then(setSeatTypes);
  }, []);

  const standardSeatTypeId = seatTypes.find((st) => st.name === "Standard")?.id;
  const vipSeatTypeId = seatTypes.find((st) => st.name === "VIP")?.id;

  // "G, H" or "G H" -> ["G", "H"], trimmed and uppercased so it matches
  // regardless of how the admin typed it.
  const vipRowLabels = vipRows
    .split(/[\s,]+/)
    .map((label) => label.trim().toUpperCase())
    .filter(Boolean);

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

    const rows = Number(rowCount);
    const seats = Number(seatsPerRow);

    // Row count has a hard ceiling: past MAX_ROWS the letter-per-row scheme
    // breaks down (no more letters to hand out).
    if (rows > MAX_ROWS) {
      setError(`Max ${MAX_ROWS} rader (A–${String.fromCharCode(65 + MAX_ROWS - 1)}).`);
      return;
    }

    // The set of row letters that will actually exist, given rowCount.
    const validRowLabels = Array.from({ length: rows }, (_, i) =>
      String.fromCharCode(65 + i),
    );

    // Named VIP rows that don't correspond to a row being created: fail
    // before creating anything, rather than silently doing nothing with them.
    const unknownVipRows = vipRowLabels.filter(
      (label) => !validRowLabels.includes(label),
    );
    if (unknownVipRows.length > 0) {
      setError(
        `Rad${unknownVipRows.length > 1 ? "erna" : "en"} ${unknownVipRows.join(", ")} finns inte` +
          ` (salongen har rader A–${validRowLabels[validRowLabels.length - 1] ?? "-"}).`,
      );
      return;
    }

    // Named VIP rows with no VIP seat type in the system: fail before
    // creating the auditorium, not after.
    if (vipRowLabels.length > 0 && !vipSeatTypeId) {
      setError("VIP-sätestyp saknas i systemet.");
      return;
    }

    createAuditorium({ name })
      .then((created) => {
        if (rows > 0 && seats > 0 && standardSeatTypeId) {
          addSeatsToAuditorium(
            created.auditoriumId,
            buildSeatRows(
              rows,
              seats,
              standardSeatTypeId,
              vipSeatTypeId,
              vipRowLabels,
            ),
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
      {error && <Message type="error">{error}</Message>}
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
          <label className="form-label">Antal rader (max {MAX_ROWS})</label>
          <input
            className="form-input"
            type="number"
            min={0}
            max={MAX_ROWS}
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
      {!auditorium && (
        <div className="form-field">
          <label className="form-label">VIP-rader (t.ex. G, H)</label>
          <input
            className="form-input"
            type="text"
            placeholder="Lämna tomt om inga VIP-platser"
            value={vipRows}
            onChange={(e) => setVipRows(e.target.value)}
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