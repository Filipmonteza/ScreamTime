import type { ReactNode } from "react";
import "./Modal.css";

type ModalProps = {
  onClose: () => void;
  children: ReactNode;
};

function Modal({ onClose, children }: ModalProps) {
  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>
  );
}

export default Modal;
