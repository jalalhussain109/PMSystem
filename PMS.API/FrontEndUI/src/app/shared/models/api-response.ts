export interface ServiceResult<T> {
  succeeded: boolean;
  notFound: boolean;
  data: T | null;
  errors: string[];
}
