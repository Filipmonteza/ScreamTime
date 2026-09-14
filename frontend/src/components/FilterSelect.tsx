import "./FilterSelect.css";
import iconChevronDown from "../assets/icons/Icon_Chevron_Down.svg";

export type FilterOption = {
  value: string;
  label: string;
};

type FilterSelectProps = {
  value: string;
  onChange: (value: string) => void;
  options: FilterOption[];
  placeholder: string;
};

function FilterSelect({
  value,
  onChange,
  options,
  placeholder,
}: FilterSelectProps) {
  const selectedLabel = options.find((o) => o.value === value)?.label;

  return (
    <div className="filter-select-wrapper">
      <span className="filter-label">{selectedLabel ?? placeholder}</span>
      <img src={iconChevronDown} alt="" className="filter-chevron" />
      <select
        className="filter-select"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}

export default FilterSelect;
