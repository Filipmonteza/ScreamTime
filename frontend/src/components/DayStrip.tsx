import { formatShortDayLabel } from "../utils/dateOptions";
import "./DayStrip.css";

type DayStripProps = {
  days: string[];
  selected: string;
  onSelect: (date: string) => void;
};

function DayStrip({ days, selected, onSelect }: DayStripProps) {
  return (
    <div className="day-strip" role="group" aria-label="Dag">
      {days.map((date) => (
        <button
          key={date}
          type="button"
          className="day-strip-day"
          aria-pressed={date === selected}
          onClick={() => onSelect(date)}
        >
          {formatShortDayLabel(date)}
        </button>
      ))}
    </div>
  );
}

export default DayStrip;
