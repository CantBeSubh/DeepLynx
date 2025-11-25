import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import TagManagementClient from "./TagManagementClient";

const TagManagementPage = async () => {
  const cookieStore = await cookies();
  const orgSessionCookie = cookieStore.get("organizationSession");

  if (!orgSessionCookie) {
    redirect("/select-org");
  }

  try {
    JSON.parse(orgSessionCookie.value);
  } catch {
    redirect("/select-org");
  }

  return <TagManagementClient />;
};

export default TagManagementPage;
