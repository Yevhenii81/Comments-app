import { Component, OnInit } from '@angular/core';
import { CommentService } from '../../services/comment.service';
import { Comment } from '../../models/comment.model';

@Component({
  selector: 'app-comment-list',
  templateUrl: './comment-list.component.html',
 standalone: false
})

export class CommentListComponent implements OnInit {
  comments: Comment[] = [];
  totalPages = 0;
  currentPage = 1;
  sortBy = 'createdAt';
  sortDesc = true;
  loading = false;

  constructor(private commentService: CommentService) {}

  ngOnInit() { this.loadComments(); }

  loadComments() {
    this.loading = true;
    this.commentService.getComments(this.currentPage, this.sortBy, this.sortDesc)
      .subscribe(result => {
        this.comments = result.items;
        this.totalPages = result.totalPages;
        this.loading = false;
      });
  }

  sort(field: string) {
    if (this.sortBy === field) {
      this.sortDesc = !this.sortDesc;
    } else {
      this.sortBy = field;
      this.sortDesc = true;
    }
    this.currentPage = 1;
    this.loadComments();
  }

  goToPage(page: number) {
    this.currentPage = page;
    this.loadComments();
  }

  get pages() {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}