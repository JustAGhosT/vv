#!/usr/bin/env python3
"""
File Name Harmonization Script for VeritasVault Documentation

This script standardizes Markdown file naming conventions across the documentation:
- Converts CamelCase and mixed case filenames to kebab-case
- Creates redirect stubs for backward compatibility
- Updates internal references in all Markdown files
- Generates a mapping report of all changes

Usage:
    python harmonize-file-names.py [--dry-run] [--docs-path PATH] [--report-path PATH]

Arguments:
    --dry-run       Run without making actual changes (default: False)
    --docs-path     Path to documentation directory (default: src/vv.Domain/Docs)
    --report-path   Path to save the mapping report (default: file-name-mapping-report.md)
"""

import os
import re
import sys
import argparse
import logging
from pathlib import Path
from datetime import datetime

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(sys.stdout),
        logging.FileHandler('harmonize-files.log')
    ]
)
logger = logging.getLogger(__name__)

# Regular expressions
CAMEL_CASE_PATTERN = re.compile(r'([a-z0-9])([A-Z])')
KEBAB_CASE_PATTERN = re.compile(r'^[a-z0-9]+(-[a-z0-9]+)*\.md$')
MARKDOWN_LINK_PATTERN = re.compile(r'\[([^\]]+)\]\(([^)]+)\)')
YAML_DEPENDENCIES_PATTERN = re.compile(r'dependencies:\s*\[(.*?)\]', re.DOTALL)

def camel_to_kebab(name):
    """Convert CamelCase to kebab-case."""
    # First handle the filename without extension
    name_part, ext = os.path.splitext(name)
    # Replace CamelCase with kebab-case
    s1 = CAMEL_CASE_PATTERN.sub(r'\1-\2', name_part)
    # Convert to lowercase
    return s1.lower() + ext.lower()

def is_kebab_case(filename):
    """Check if a filename is already in kebab-case."""
    return bool(KEBAB_CASE_PATTERN.match(filename.lower()))

def needs_conversion(filename):
    """Determine if a filename needs conversion to kebab-case."""
    # Skip files that are already kebab-case
    if is_kebab_case(filename):
        return False
    
    # Check for uppercase letters or mixed case
    name_part, _ = os.path.splitext(filename)
    return any(c.isupper() for c in name_part) or '_' in name_part

def create_redirect_stub(old_path, new_path, dry_run=False):
    """Create a redirect stub file for backward compatibility."""
    if dry_run:
        logger.info(f"[DRY RUN] Would create redirect stub: {old_path} -> {new_path}")
        return
    
    # Get relative path for the link
    rel_path = os.path.relpath(new_path, os.path.dirname(old_path))
    
    # Create the redirect content
    redirect_content = f"""---
document_type: redirect
classification: internal
status: approved
version: 1.0.0
last_updated: {datetime.now().strftime('%Y-%m-%d')}
applies_to: [platform-wide]
---

# Redirect Notice

This document has been moved to [{os.path.basename(new_path)}]({rel_path}) as part of the file naming convention standardization.

Please update your bookmarks and references.

---

<meta http-equiv="refresh" content="0;url={rel_path}">
"""
    
    # Create the directory if it doesn't exist
    os.makedirs(os.path.dirname(old_path), exist_ok=True)
    
    # Write the redirect file
    with open(old_path, 'w', encoding='utf-8') as f:
        f.write(redirect_content)
    
    logger.info(f"Created redirect stub: {old_path} -> {new_path}")

def update_markdown_links(content, file_mapping):
    """Update markdown links in content based on file mapping."""
    def replace_link(match):
        link_text = match.group(1)
        link_path = match.group(2)
        
        # Skip external links or anchors
        if link_path.startswith(('http://', 'https://', '#')):
            return match.group(0)
        
        # Parse the link path
        link_dir, link_file = os.path.split(link_path)
        
        # Check if this file was renamed
        if link_file in file_mapping:
            new_link_file = file_mapping[link_file]
            new_link_path = os.path.join(link_dir, new_link_file)
            return f'[{link_text}]({new_link_path})'
        
        return match.group(0)
    
    # Update markdown links
    updated_content = MARKDOWN_LINK_PATTERN.sub(replace_link, content)
    
    # Update YAML dependencies
    def replace_dependencies(match):
        deps_text = match.group(1)
        for old_name, new_name in file_mapping.items():
            # Only replace exact filename matches to avoid partial replacements
            # Escape both old and new names to handle special regex characters
            deps_text = re.sub(
                r'([\'"]?)' + re.escape(old_name) + r'([\'"]?)', 
                r'\1' + re.escape(new_name).replace('\\', r'\\') + r'\2', 
                deps_text
            )
        return f'dependencies: [{deps_text}]'
    
    updated_content = YAML_DEPENDENCIES_PATTERN.sub(replace_dependencies, updated_content)
    
    return updated_content

def generate_mapping_report(file_mapping, report_path):
    """Generate a markdown report of all file name changes."""
    report_content = f"""# File Name Harmonization Report
Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

This report documents all file name changes made to standardize documentation file naming conventions.

## Naming Convention Standard
- All documentation files now use **kebab-case** (lowercase with hyphens)
- Example: `BlackLitterman-Implementation.md` → `black-litterman-implementation.md`

## File Mapping

| Original Filename | New Filename | Status |
|-------------------|--------------|--------|
"""
    
    # Sort the mapping for better readability
    sorted_mapping = {k: file_mapping[k] for k in sorted(file_mapping.keys())}
    
    for old_name, new_name in sorted_mapping.items():
        report_content += f"| {old_name} | {new_name} | ✅ Renamed |\n"
    
    report_content += f"""
## Impact

- **Total files renamed**: {len(file_mapping)}
- **Redirect stubs created**: {len(file_mapping)}
- **Documentation structure**: Standardized to kebab-case

## Next Steps

1. Update any external references to these files
2. Review the redirect stubs for correctness
3. Run link checker to verify all internal references are working

---

*This report was generated automatically by the `harmonize-file-names.py` script.*
"""
    
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report_content)
    
    logger.info(f"Generated mapping report at: {report_path}")

def main():
    # Parse command line arguments
    parser = argparse.ArgumentParser(description='Harmonize file naming conventions in documentation.')
    parser.add_argument('--dry-run', action='store_true', help='Run without making actual changes')
    parser.add_argument('--docs-path', default='src/vv.Domain/Docs', help='Path to documentation directory')
    parser.add_argument('--report-path', default='file-name-mapping-report.md', help='Path to save the mapping report')
    args = parser.parse_args()
    
    docs_path = Path(args.docs_path)
    report_path = args.report_path
    dry_run = args.dry_run
    
    if dry_run:
        logger.info("Running in DRY RUN mode - no changes will be made")
    
    # Validate docs path
    if not docs_path.exists() or not docs_path.is_dir():
        logger.error(f"Documentation directory not found: {docs_path}")
        return 1
    
    # Dictionary to store file mappings (old_name -> new_name)
    file_mapping = {}
    
    # Step 1: Identify files that need renaming
    logger.info("Scanning for files that need renaming...")
    md_files = []
    
    for root, _, files in os.walk(docs_path):
        for filename in files:
            if filename.lower().endswith('.md'):
                full_path = os.path.join(root, filename)
                md_files.append(full_path)
                
                if needs_conversion(filename):
                    new_filename = camel_to_kebab(filename)
                    file_mapping[filename] = new_filename
                    logger.info(f"Found file to rename: {filename} -> {new_filename}")
    
    logger.info(f"Found {len(file_mapping)} files to rename out of {len(md_files)} total markdown files")
    
    if not file_mapping:
        logger.info("No files need renaming. Exiting.")
        return 0
    
    # Step 2: Rename files and create redirect stubs
    if not dry_run:
        logger.info("Renaming files and creating redirect stubs...")
        
        for root, _, files in os.walk(docs_path):
            for filename in files:
                if filename in file_mapping:
                    old_path = os.path.join(root, filename)
                    new_filename = file_mapping[filename]
                    new_path = os.path.join(root, new_filename)
                    
                    # Read the content before renaming
                    with open(old_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    # Rename the file
                    os.rename(old_path, new_path)
                    logger.info(f"Renamed: {old_path} -> {new_path}")
                    
                    # Write the content to the new file
                    with open(new_path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    
                    # Create redirect stub
                    create_redirect_stub(old_path, new_path, dry_run)
    
    # Step 3: Update internal references in all markdown files
    logger.info("Updating internal references...")
    
    for md_file in md_files:
        try:
            with open(md_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            updated_content = update_markdown_links(content, file_mapping)
            
            if content != updated_content:
                if not dry_run:
                    with open(md_file, 'w', encoding='utf-8') as f:
                        f.write(updated_content)
                    logger.info(f"Updated references in: {md_file}")
                else:
                    logger.info(f"[DRY RUN] Would update references in: {md_file}")
        except Exception as e:
            logger.error(f"Error updating references in {md_file}: {str(e)}")
    
    # Step 4: Generate mapping report
    if not dry_run:
        generate_mapping_report(file_mapping, report_path)
    else:
        logger.info(f"[DRY RUN] Would generate mapping report at: {report_path}")
    
    logger.info("File name harmonization completed successfully")
    return 0

if __name__ == "__main__":
    sys.exit(main())
