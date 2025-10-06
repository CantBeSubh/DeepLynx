// "use client";

// import api from "./api";

// export async function getAllRoles(projectId: number, organizationId?: number, hideArchived?: boolean) {
//   const params = new URLSearchParams();
//   params.append('projectId', projectId.toString());

//   if (organizationId !== undefined) {
//     params.append('organizationId', organizationId.toString());
//   }

//   if (hideArchived !== undefined) {
//     params.append('hideArchived', hideArchived.toString());
//   }

//   const res = await api.get(`/roles/GetAllRoles?${params.toString()}`);
//   return res.data;
// }