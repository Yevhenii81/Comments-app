import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Comment } from '../../models/comment.model';
import { LightboxComponent } from '../lightbox/lightbox.component';

@Component({
  selector: 'app-comment-item',
  templateUrl: './comment-item.component.html',
  standalone: false
})

export class CommentItemComponent {
  @Input() comment!: Comment;
  @Output() commentAdded = new EventEmitter<void>();
  @ViewChild(LightboxComponent) lightbox!: LightboxComponent;

  showReplyForm = false;

  formatCommentText(text: string): string {
    if (!text) { return ''; }
    if (/<\/?.+?>/.test(text)) {
      return text;
    }

    const escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    return escaped
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.+?)\*/g, '<em>$1</em>')
      .replace(/`(.+?)`/g, '<code>$1</code>');
  }

  toggleReplyForm() {
    this.showReplyForm = !this.showReplyForm;
  }

  onReplyAdded() {
    this.showReplyForm = false;
    this.commentAdded.emit();
  }

  getFileUrl(filePath: string): string {
    if (!filePath) {
      return '';
    }

    if (filePath.startsWith('http://') || filePath.startsWith('https://')) {
      return filePath;
    }

    if (filePath.startsWith('/')) {
      return `https://localhost:7187${filePath}`;
    }

    return `https://localhost:7187/${filePath}`;
  }

  openLightbox(url: string) {
    const fullUrl = this.getFileUrl(url);
    console.log('openLightbox', fullUrl);
    this.lightbox.open(fullUrl);
  }
}