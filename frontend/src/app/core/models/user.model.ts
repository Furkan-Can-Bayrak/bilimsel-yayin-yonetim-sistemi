/** Backend AcademicTitle enum değerleriyle birebir. */
export const AcademicTitle = {
  ProfDr: 1,
  DocDr: 2,
  DrOgrUyesi: 3,
  OgrGor: 4,
  ArsGor: 5,
  Dr: 6,
} as const;

export type AcademicTitleValue = (typeof AcademicTitle)[keyof typeof AcademicTitle];

export const AcademicTitleOptions: ReadonlyArray<{ value: AcademicTitleValue; label: string }> = [
  { value: AcademicTitle.ProfDr, label: 'Prof. Dr.' },
  { value: AcademicTitle.DocDr, label: 'Doç. Dr.' },
  { value: AcademicTitle.DrOgrUyesi, label: 'Dr. Öğr. Üyesi' },
  { value: AcademicTitle.OgrGor, label: 'Öğr. Gör.' },
  { value: AcademicTitle.ArsGor, label: 'Arş. Gör.' },
  { value: AcademicTitle.Dr, label: 'Dr.' },
];

export interface UserListItem {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  academicTitle: AcademicTitleValue;
  isActive: boolean;
  roleNames: string[];
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  academicTitle: AcademicTitleValue;
  orcid: string | null;
  institutionId: number | null;
  roleIds: number[];
}

export interface CreateUserResult {
  id: number;
  email: string;
}

export interface RoleListItem {
  id: number;
  name: string;
}

export function academicTitleLabel(value: AcademicTitleValue): string {
  return AcademicTitleOptions.find((o) => o.value === value)?.label ?? 'Dr.';
}
