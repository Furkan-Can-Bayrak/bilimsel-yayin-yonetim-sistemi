/** API JsonStringEnumConverter ile AcademicTitle adlarını string gönderir. */
export type AcademicTitleValue =
  | 'ProfDr'
  | 'DocDr'
  | 'DrOgrUyesi'
  | 'OgrGor'
  | 'ArsGor'
  | 'Dr';

export const AcademicTitle = {
  ProfDr: 'ProfDr',
  DocDr: 'DocDr',
  DrOgrUyesi: 'DrOgrUyesi',
  OgrGor: 'OgrGor',
  ArsGor: 'ArsGor',
  Dr: 'Dr',
} as const satisfies Record<AcademicTitleValue, AcademicTitleValue>;

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
  roleIds: number[];
  roleNames: string[];
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  academicTitle: AcademicTitleValue;
  orcid: string | null;
  institutionId: number;
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

export interface InstitutionListItem {
  id: number;
  name: string;
  emailDomain: string;
}

export function academicTitleLabel(value: AcademicTitleValue | string | number | null | undefined): string {
  if (value == null) {
    return 'Dr.';
  }

  if (typeof value === 'number') {
    const byNumber: Record<number, AcademicTitleValue> = {
      1: 'ProfDr',
      2: 'DocDr',
      3: 'DrOgrUyesi',
      4: 'OgrGor',
      5: 'ArsGor',
      6: 'Dr',
    };
    const name = byNumber[value];
    return AcademicTitleOptions.find((o) => o.value === name)?.label ?? 'Dr.';
  }

  return AcademicTitleOptions.find((o) => o.value === value)?.label ?? 'Dr.';
}

function foldTurkishChar(c: string): string {
  const map: Record<string, string> = {
    ç: 'c',
    Ç: 'c',
    ğ: 'g',
    Ğ: 'g',
    ı: 'i',
    I: 'i',
    İ: 'i',
    ö: 'o',
    Ö: 'o',
    ş: 's',
    Ş: 's',
    ü: 'u',
    Ü: 'u',
  };
  return map[c] ?? c.toLowerCase();
}

function foldToAscii(value: string): string {
  return [...value]
    .map(foldTurkishChar)
    .join('')
    .normalize('NFD')
    .replace(/\p{M}/gu, '')
    .replace(/[^a-z0-9]/gi, '')
    .toLowerCase();
}

/** Backend UserEmailHelper.BuildLocalPart ile aynı kural (önizleme; yazıldıkça büyür). */
export function buildEmailLocalPart(firstName: string, lastName: string): string {
  const initials = firstName
    .trim()
    .split(/\s+/)
    .map((part) => foldToAscii(part))
    .filter((part) => part.length > 0)
    .map((part) => part[0]);

  const surname = foldToAscii(lastName.trim());
  return initials.join('') + surname;
}

export function buildEmailPreview(
  firstName: string,
  lastName: string,
  emailDomain: string | null | undefined,
): string {
  const local = buildEmailLocalPart(firstName, lastName);
  const domain = (emailDomain ?? '').trim().replace(/^@/, '').toLowerCase();

  if (!local && !domain) {
    return '';
  }

  if (!domain) {
    return local;
  }

  if (!local) {
    return `@${domain}`;
  }

  return `${local}@${domain}`;
}
