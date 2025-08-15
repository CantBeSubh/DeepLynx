// app/(home)/PageAreaSkeleton.tsx
import Skeleton from "react-loading-skeleton";

export default function PageAreaSkeleton() {
  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <Skeleton width={240} height={28} />
        <Skeleton width={200} height={36} />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="card p-4">
          <Skeleton height={180} />
        </div>
        <div className="card p-4">
          <Skeleton height={180} />
        </div>
      </div>
    </div>
  );
}
