// src/app/(home)/project_management/page.tsx

import { redirect } from "next/navigation";

const ProjectManagementIndexPage = async () => {
  // No project id provided: send them home (or wherever you want)
  redirect("/");
};

export default ProjectManagementIndexPage;
