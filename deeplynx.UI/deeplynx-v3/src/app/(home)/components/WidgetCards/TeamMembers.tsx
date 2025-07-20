// import React, { useState, useEffect } from "react";
// import Image from 'next/image';
// import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal"
// import {ChevronDownIcon, ChevronRightIcon, ChevronLeftIcon} from "@heroicons/react/24/outline";
// import {PlusCircleIcon} from "@heroicons/react/24/solid";
// import AvatarCarousel from "./WidgetCardModals/AvatarCarousel";
// import { peopleData } from "../dummy_data/data";

// const TeamMembersWidget: React.FC = () => {
//     const [showTable, setShowTable] = useState(false);
//     const [addMemberModal, setAddMemberModal] = useState(false);
//     // const avatars = Array.from({ length: 30 }, (_, i) => `https://i.pravatar.cc/150?img=${i + 1}`);
//     const [avatars, setAvatars] = useState<string[]>([]);

//     useEffect(() => {
//         console.log("peopleData:", peopleData);

//         // Ensure peopleData is an array and map the avatars
//         if (Array.isArray(peopleData)) {
//             setAvatars(peopleData.map(person => person.avatar));
//         } else {
//             console.error("peopleData is not an array");
//         }
//     }, []);

//     const handleToggle = () => {
//         setShowTable((prev) => !prev);
//     };

//     return (
//         <div className="card-body">
//             <div className="flex justify-between">
//                 <h2 className="card-title flex items-center">
//                     Team Members
//                     {/* Show Plus Icon only when table is shown */}
//                     {showTable && (
//                         <button onClick={() => setAddMemberModal(true)} className="ml-1">
//                             <PlusCircleIcon
//                                 className="w-7 h-7 text-secondary" />
//                         </button>
//                     )}
//                 </h2>
//                 <button onClick={handleToggle} className="btn btn-sm btn-ghost">
//                     {showTable ? (
//                         <ChevronDownIcon
//                             className="size-6 rotate-180"
//                         />
//                     ) : (
//                         <ChevronDownIcon
//                             className="size-6"
//                         />
//                     )}
//                 </button>
//             </div>

//             {!showTable ? (
//                 <AvatarCarousel avatars={avatars} />
//             ) : (
//                 // Team Members Table
//                 <div className="overflow-x-auto">
//                     <div className="flex justify-between items-center">
//                     </div>
//                     <table className="table">
//                     {/* head */}
//                         <thead>
//                             <tr className="text-secondary-content">
//                             <th className="pl-17 flex items-center">
//                                 Name
//                                 <div className="pl-2">
//                                     <ChevronDownIcon
//                                         className="size-5"
//                                     />
//                                 </div>
//                             </th>
//                             <th>Role</th>
//                             <th className="flex items-center">
//                                 Last Login
//                                 <div className="pl-2">
//                                     <ChevronDownIcon
//                                         className="size-5"
//                                     />
//                                 </div>
//                             </th>
//                             <th></th>
//                             </tr>
//                         </thead>
//                     <tbody>
//                         {peopleData.map((person, index) => (
//                             <tr key={index}>
//                                 <td>
//                                     <div className="flex items-center gap-3">
//                                         <div className="avatar">
//                                             <div className="mask mask-circle h-10 w-10">
//                                                 <Image
//                                                     src={person.avatar}
//                                                     alt={`${person.name}'s avatar`}
//                                                     width="300"
//                                                     height="300"
//                                                 />
//                                             </div>
//                                         </div>
//                                         <div>
//                                             <div className="">{person.name}</div>
//                                         </div>
//                                     </div>
//                                 </td>
//                                 <td>
//                                     {person.role}
//                                     <br />
//                                 </td>
//                                 <td>{person.lastLogin}</td>
//                             </tr>
//                         ))}
//                     </tbody>
//                     </table>

//                     {/* Table Toggle Buttons */}
//                     <div className="flex justify-end">
//                         <button className="btn join-item p-2 rounded-r-none">
//                             <ChevronLeftIcon
//                                 className="size-6" />
//                         </button>
//                         <button className="btn join-item p-2 rounded-l-none">
//                             <ChevronRightIcon
//                                 className="size-6" />
//                         </button>
//                     </div>
//                 </div>
//             )}

//             {/* Create Link Modal */}
//             <AddMember
//                 isOpen={addMemberModal}
//                 onClose={() => setAddMemberModal(false)}
//             />

//         </div>
//     );
// };

// export default TeamMembersWidget;