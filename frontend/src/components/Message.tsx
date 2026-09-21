import { useEffect, useState } from "react";
import "./Message.css";
import iconSuccess from "../assets/icons/Icon_Success.svg";
import iconWarning from "../assets/icons/Icon_Warning.svg";
import iconError from "../assets/icons/Icon_Error.svg";

type MessageType = "success" | "warning" | "error";

type MessageProps = {
  type: MessageType;
  children: React.ReactNode;
};

const icons: Record<MessageType, string> = {
  success: iconSuccess,
  warning: iconWarning,
  error: iconError,
};

const AUTO_DISMISS_MS = 4000;

function Message({ type, children }: MessageProps) {
  const [visible, setVisible] = useState(true);

  // Only confirmations fade out. An error has to stay until whatever showed it
  // clears it: the same error shown twice in a row is identical by props, so a
  // dismissed Message would have no way of knowing it should come back.
  useEffect(() => {
    if (type !== "success") return;
    const timer = setTimeout(() => setVisible(false), AUTO_DISMISS_MS);
    return () => clearTimeout(timer);
  }, [type, children]);

  if (!visible) return null;
  return (
    <div className={`message message-${type}`}>
      <img src={icons[type]} alt="" className="message-icon" />
      <span className="message-text">{children}</span>
    </div>
  );
}

export default Message;
