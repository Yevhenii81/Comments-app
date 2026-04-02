export interface User {
  userName: string;
  email: string;
  homePage?: string;
}

export interface Comment {
  id: number;
  text: string;
  createdAt: string;
  filePath?: string;
  fileType?: string;
  parentId?: number;
  user: User;
  replies: Comment[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CaptchaResponse {
  token: string;
  imageBase64: string;
}