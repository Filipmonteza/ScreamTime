import { useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Auditorium } from "../../types/auditorium";
import { createAuditorium, updateAuditorium } from "../../api/auditoriumsApi";

type AuditoriumFormProps = {
  auditorium?: Auditorium;
  onClose: () => void;
  onSaved: () => void;
};

function AuditoriumForm({ auditorium, onClose, onSaved }: AuditoriumFormProps) {
  const [name, setName] = useState(auditorium?.name ?? "");
  const [isActive, setIsActive] = useState(auditorium?.isActive ?? true);

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    const request = auditorium
      ? updateAuditorium(auditorium.auditoriumId, { name, isActive })
      : createAuditorium({ name });
    request.then(onSaved);
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
