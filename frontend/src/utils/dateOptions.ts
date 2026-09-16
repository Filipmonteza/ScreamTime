function formatDateKey(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

export function formatDateTimeLabel(iso: string): string {
  const date = formatDateOptionLabel(dateKeyFromIso(iso), 2);
  const time = new Date(iso).toLocaleTimeString("sv-SE", {
    hour: "2-digit",
    minute: "2-digit",
  });
  return `${date}, ${time}`;
}

export function dateKeyFromIso(iso: string): string {
    return iso.slice(0, 10);
}

export function todayKey(): string {
    return formatDateKey(new Date());
}

export function nextDays(count: number): string[] {
    const today = new Date();
    return Array.from({ length: count }, (_, i) => {
        const date = new Date(
            today.getFullYear(),
            today.getMonth(),
            today.getDate() + i,
        );
        return formatDateKey(date);
    });
}

export function formatWeekdayLabel(dateStr: string): string {
    const weekday = new Date(dateStr).toLocaleDateString("sv-SE", {
        weekday: "long",
    });
    return weekday.charAt(0).toUpperCase() + weekday.slice(1);
}

export function formatDateOptionLabel(dateStr: string, index: number): string {
    if (index === 0) return "Idag";
    if (index === 1) return "Imorgon";
    const date = new Date(dateStr);
    const weekday = date.toLocaleDateString("sv-SE", { weekday: "long" });
    const dayMonth = date.toLocaleDateString("sv-SE", {
        day: "numeric",
        month: "long",
    });
    const capitalizedWeekday = weekday.charAt(0).toUpperCase() + weekday.slice(1);
    return `${capitalizedWeekday} ${dayMonth}`;
}