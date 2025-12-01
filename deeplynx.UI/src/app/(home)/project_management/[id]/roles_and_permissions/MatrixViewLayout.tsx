"use client";

import React, { useRef } from "react";
import {
  BuildingOfficeIcon,
  CheckIcon,
  ShieldCheckIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";

import {
  PermissionResponseDto,
  RoleResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { PermissionCategory } from "./ProjectRolesAndPermissions";

interface MatrixViewLayoutProps {
  roles: RoleResponseDto[];
  permissionCategories: PermissionCategory[];
  roleHasPermission: (roleId: number, permissionId: number) => boolean;
  isStandardRole: (role: RoleResponseDto) => boolean;
  isOrganizationRole: (role: RoleResponseDto) => boolean;
  isProjectRole: (role: RoleResponseDto) => boolean;
  getRoleSource: (role: RoleResponseDto) => string;
  isLoadingPermissions: boolean;
  initialLoadComplete: boolean;
}

const MatrixViewLayout: React.FC<MatrixViewLayoutProps> = ({
  roles,
  permissionCategories,
  roleHasPermission,
  isStandardRole,
  isOrganizationRole,
  isProjectRole,
  getRoleSource,
  isLoadingPermissions,
  initialLoadComplete,
}) => {
  const tableContainerRef = useRef<HTMLDivElement>(null);

  return (
    <div style={{ height: "calc(100vh - 28rem)" }}>
      <div className="card bg-base-100 shadow-xl h-full flex flex-col overflow-hidden">
        <div className="px-6 py-3 border-b border-base-300 flex items-center justify-between">
          <h3 className="text-sm font-semibold">Permission Matrix</h3>
          {/* Read-only matrix in this release */}
        </div>

        {isLoadingPermissions && !initialLoadComplete ? (
          <div className="flex-1 flex items-center justify-center">
            <div className="text-center">
              <span className="loading loading-spinner loading-lg text-primary"></span>
              <p className="mt-4 text-base-content/60">
                Loading permissions...
              </p>
            </div>
          </div>
        ) : (
          <div ref={tableContainerRef} className="flex-1 overflow-auto">
            <table className="table">
              <thead className="sticky top-0 z-20 bg-base-100">
                <tr>
                  <th className="sticky left-0 bg-base-200 z-30">Permission</th>
                  {roles.map((role) => (
                    <th key={role.id} className="text-center">
                      <div className="flex flex-col items-center gap-1">
                        <div className="flex items-center gap-2">
                          <ShieldCheckIcon className="w-4 h-4 text-primary" />
                          <span className="font-medium">{role.name}</span>
                          {isStandardRole(role) && (
                            <div className="badge badge-info badge-xs">STD</div>
                          )}
                          {isOrganizationRole(role) && (
                            <div className="badge badge-secondary badge-xs flex gap-1">
                              <BuildingOfficeIcon className="w-3 h-3" />
                              ORG
                            </div>
                          )}
                          {isProjectRole(role) && (
                            <div className="badge badge-primary badge-xs">
                              PRJ
                            </div>
                          )}
                        </div>
                        {role.description && (
                          <span className="text-xs text-base-content/60 font-normal">
                            {role.description}
                          </span>
                        )}
                        <span className="text-xs text-base-content/50 font-normal">
                          {getRoleSource(role)}
                        </span>
                      </div>
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {permissionCategories.map((category) => (
                  <React.Fragment key={category.id}>
                    <tr className="bg-base-200">
                      <td
                        colSpan={roles.length + 2}
                        className="font-semibold text-sm sticky left-0"
                      >
                        {category.label}
                      </td>
                    </tr>
                    {category.permissions.map((perm: PermissionResponseDto) => (
                      <tr key={perm.id} className="hover">
                        <td className="sticky left-0 z-10 bg-base-100">
                          <div className="flex flex-col">
                            <span className="font-medium text-sm">
                              {perm.name}
                            </span>
                            {perm.description && (
                              <span className="text-xs text-base-content/60">
                                {perm.description}
                              </span>
                            )}
                            <span className="text-xs text-base-content/50 mt-1">
                              Action: {perm.action}
                            </span>
                          </div>
                        </td>
                        {roles.map((role) => {
                          const hasPermission = roleHasPermission(
                            role.id,
                            Number(perm.id)
                          );

                          return (
                            <td key={role.id} className="text-center">
                              <div
                                className="inline-block cursor-default"
                                title={
                                  hasPermission
                                    ? "Has permission"
                                    : "No permission"
                                }
                              >
                                {hasPermission ? (
                                  <CheckIcon className="size-8 mx-auto text-success" />
                                ) : (
                                  <XMarkIcon className="size-8 mx-auto text-base-300" />
                                )}
                              </div>
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </React.Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default MatrixViewLayout;
