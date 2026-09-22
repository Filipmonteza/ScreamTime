import type { ReactNode, SubmitEvent } from "react";
import Modal from "./Modal";
import "./Form.css";

type FormProps = {
  title: string;
  submitLabel: string;
  onSubmit: (e: SubmitEvent<HTMLFormElement>) => void;
  onCancel: () => void;
  children: ReactNode;
};

function Form({ title, submitLabel, onSubmit, onCancel, children }: FormProps) {
  return (
    <Modal onClose={onCancel}>
      <form onSubmit={onSubmit}>
        <h1 className="form-title">{title}</h1>
        {children}
        <div className="form-actions">
          <button type="submit" className="btn-pill">
            {submitLabel}
          </button>
          <button type="button" className="btn-pill" onClick={onCancel}>
            Avbryt
          </button>
        </div>
      </form>
    </Modal>
  );
}

export default Form;