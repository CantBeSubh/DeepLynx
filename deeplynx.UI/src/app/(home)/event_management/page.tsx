import EventHistory from "./EventHistory"

const EventManagementPage = () => {
  return (
    <section>
      <h1 className="p-2 text-2xl font-bold text-info-content" >Event Management</h1>
      <EventHistory/>
    </section>
  )
}

export default EventManagementPage