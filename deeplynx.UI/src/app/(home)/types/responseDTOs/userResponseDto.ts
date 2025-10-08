export type UserResponseDto =
  {
    id: number;
    name: string;
    email: string;
    username: string;
    ssoId: string;
    isSysAdmin: boolean;
    isArchived: boolean;
    isActive: boolean;
  }