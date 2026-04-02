import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment, PagedResult, CaptchaResponse } from '../models/comment.model';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) {}

  getComments(page = 1, sortBy = 'createdAt', sortDesc = true): Observable<PagedResult<Comment>> {
    const params = new HttpParams()
      .set('page', page)
      .set('sortBy', sortBy)
      .set('sortDesc', sortDesc);
    return this.http.get<PagedResult<Comment>>(`${this.apiUrl}/comments`, { params });
  }

  createComment(formData: FormData): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/comments`, formData);
  }

  getCaptcha(): Observable<CaptchaResponse> {
    return this.http.get<CaptchaResponse>(`${this.apiUrl}/captcha`);
  }
}