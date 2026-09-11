import { useEffect, useMemo, useState } from "react";
import type { Auditorium } from "../../types/auditorium";
import { getAllAuditoriums, deleteAuditorium, updateAuditorium } from "../../api/auditoriumsApi";
import AuditoriumForm from "./AuditoriumForm";
import SearchBar from "../../components/SearchBar";
import ListToolbar from "../../components/ListToolbar";
import usePagination from "../../hooks/usePagination";
import ListCard from "../../components/ListCard";


function Auditoriums() {
  const [auditoriums, setAuditoriums] = useState<Auditorium[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(
    null,
  );
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingAuditorium, setEditingAuditorium] = useState<Auditorium | null>(
    null,
  );
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [confirmingActivateId, setConfirmingActivateId] = useState<
    string | null
  >(null);
  const [activatingId, setActivatingId] = useState<string | null>(null);

  function loadAuditoriums() {
    getAllAuditoriums(true)
      .then((data) => {
        setError(null);
        setAuditoriums(data);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadAuditoriums();
  }, []);

  function handleDelete(id: string) {
    setError(null);
    setDeletingId(id);
    deleteAuditorium(id)
      .then(() => {
        setConfirmingDeleteId(null);
        loadAuditoriums();
      })
      .catch((err) => {
        setConfirmingDeleteId(null);
        setError(err.message);
      })
      .finally(() => setDeletingId(null));
  }

  function handleReactivate(auditorium: Auditorium) {
    setError(null);
    setActivatingId(auditorium.auditoriumId);
    updateAuditorium(auditorium.auditoriumId, {
      name: auditorium.name,
      isActive: true,
    })
      .then(() => {
        setConfirmingActivateId(null);
        loadAuditoriums();
      })
      .catch((err) => {
        setConfirmingActivateId(null);
        setError(err.message);
      })
      .finally(() => setActivatingId(null));
  }

  const filteredAuditoriums = useMemo(() => {
    const q = query.trim().toLowerCase();
    return auditoriums.filter((auditorium) =>
      auditorium.name.toLowerCase().includes(q),
    );
  }, [auditoriums, query]);

  const {
    page,
    setPage,
    totalPages,
    pagedItems: pagedAuditoriums,
  } = usePagination(filteredAuditoriums, 12, query);

  return (
    <div className="page auditoriums">
      <h1>Salonger</h1>
      <SearchBar value={query} onChange={setQuery} placeholder="Sök salong" />
      <ListToolbar
        addLabel="Lägg till salong"
        onAdd={() => setShowAddForm(true)}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}
      {!loading && (
        <div className="card-grid">
          {pagedAuditoriums.map((auditorium) => (
            <ListCard
              key={auditorium.auditoriumId}
              title={auditorium.name}
              infoLines={[`${auditorium.capacity} platser`]}
              confirming={confirmingDeleteId === auditorium.auditoriumId}
              deleting={deletingId === auditorium.auditoriumId}
              deleteLabel="Inaktivera"
              isActive={auditorium.isActive}
              confirmingReactivate={confirmingActivateId === auditorium.auditoriumId}
              reactivating={activatingId === auditorium.auditoriumId}
              onEdit={() => setEditingAuditorium(auditorium)}
              onRequestDelete={() =>
                setConfirmingDeleteId(auditorium.auditoriumId)
              }
              onConfirmDelete={() => handleDelete(auditorium.auditoriumId)}
              onCancelDelete={() => setConfirmingDeleteId(null)}
              onRequestReactivate={() =>
                setConfirmingActivateId(auditorium.auditoriumId)
              }
              onConfirmReactivate={() => handleReactivate(auditorium)}
              onCancelReactivate={() => setConfirmingActivateId(null)}
            />
          ))}
        </div>
      )}

      {showAddForm && (
        <AuditoriumForm
          onClose={() => setShowAddForm(false)}
          onSaved={() => {
            setShowAddForm(false);
            loadAuditoriums();
          }}
        />
      )}
      {editingAuditorium && (
        <AuditoriumForm
          auditorium={editingAuditorium}
          onClose={() => setEditingAuditorium(null)}
          onSaved={() => {
            setEditingAuditorium(null);
            loadAuditoriums();
          }}
        />
      )}
    </div>
  );
}

export default Auditoriums;
