import { useMemo, useState } from "react";

function usePagination<T>(items: T[], pageSize: number, resetKey: string | number) {
    const [page, setPage] = useState(1);

    const totalPages = Math.max(1, Math.ceil(items.length / pageSize));

    if (page > totalPages) {
        setPage(totalPages);
    }

    const [prevResetKey, setPrevResetKey] = useState(resetKey);
    if (resetKey !== prevResetKey) {
        setPrevResetKey(resetKey);
        setPage(1);
    }

    const pagedItems = useMemo(() => {
        const start = (page - 1) * pageSize;
        return items.slice(start, start + pageSize);
    }, [items, page, pageSize]);

    return { page, setPage, totalPages, pagedItems };
}

export default usePagination;