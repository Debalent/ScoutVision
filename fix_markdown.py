#!/usr/bin/env python3
"""
Markdown Linting Fixer for ScoutVision Repository
Fixes common markdown linting issues automatically.
"""

import os
import re
import glob

def fix_markdown_file(file_path):
    """Fix common markdown linting issues in a single file."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Fix MD022: Add blank lines around headings
    content = re.sub(r'([^\n])\n(#{1,6}\s)', r'\1\n\n\2', content)
    content = re.sub(r'(#{1,6}.*)\n([^#\n])', r'\1\n\n\2', content)
    
    # Fix MD032: Add blank lines around lists
    content = re.sub(r'([^\n])\n([-*]\s)', r'\1\n\n\2', content)
    content = re.sub(r'([^\n])\n(\d+\.\s)', r'\1\n\n\2', content)
    content = re.sub(r'([-*]\s.*)\n([^-*\s\n])', r'\1\n\n\2', content)
    content = re.sub(r'(\d+\.\s.*)\n([^\d\s\n])', r'\1\n\n\2', content)
    
    # Fix MD031: Add blank lines around fenced code blocks
    content = re.sub(r'([^\n])\n(```)', r'\1\n\n\2', content)
    content = re.sub(r'(```[^\n]*)\n([^`\n])', r'\1\n\n\2', content)
    
    # Fix MD034: Wrap bare URLs in angle brackets
    content = re.sub(r'(\s)(https?://[^\s<>]+)(\s)', r'\1<\2>\3', content)
    
    # Fix MD009: Remove trailing spaces
    content = re.sub(r' +\n', '\n', content)
    
    # Fix MD040: Add language to fenced code blocks without language
    content = re.sub(r'\n```\n', '\n```text\n', content)
    
    # Fix MD036: Replace emphasis used as heading
    content = re.sub(r'\*\*(.*)\*\*\n(?=\n)', r'## \1\n', content)
    
    # Clean up multiple consecutive blank lines
    content = re.sub(r'\n{3,}', '\n\n', content)
    
    # Only write if content changed
    if content != original_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    """Fix markdown files in the repository."""
    repo_root = r"c:\Users\Admin\Documents\GitHub\ScoutVision"
    
    # Find all markdown files
    md_files = []
    for root, dirs, files in os.walk(repo_root):
        for file in files:
            if file.endswith('.md'):
                md_files.append(os.path.join(root, file))
    
    fixed_files = []
    for file_path in md_files:
        try:
            if fix_markdown_file(file_path):
                fixed_files.append(file_path)
                print(f"Fixed: {file_path}")
        except Exception as e:
            print(f"Error fixing {file_path}: {e}")
    
    print(f"\nFixed {len(fixed_files)} files out of {len(md_files)} markdown files.")

if __name__ == "__main__":
    main()