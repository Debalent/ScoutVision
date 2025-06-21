"""
ScoutVision Logo Enhancement Script

This script enhances the ScoutVision logo video for investor presentations and high-visibility use.
Enhancements include:
- High-quality upscaling
- Professional color grading
- Soft glow effect for emphasis

Requirements:
- Python 3.x
- ffmpeg installed and available in system PATH

Author: [Your Name]
Date: [Today's Date]
"""

import subprocess
import sys

# Define file paths
INPUT_PATH = "scoutvision/assets/logo/scoutvision-logo.mp4"
UPSCALED_PATH = "scoutvision/assets/logo/scoutvision-logo-upscaled.mp4"
GRADED_PATH = "scoutvision/assets/logo/scoutvision-logo-graded.mp4"
ENHANCED_PATH = "scoutvision/assets/logo/scoutvision-logo-enhanced.mp4"

def run_ffmpeg(command, description):
    """Run an ffmpeg command and handle errors gracefully."""
    print(f"\n[INFO] {description}...")
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if result.returncode != 0:
        print(f"[ERROR] {description} failed:\n{result.stderr.decode()}")
        sys.exit(1)
    print(f"[SUCCESS] {description} complete.")

def upscale_video(input_path, output_path):
    """
    Upscale the video using the lanczos algorithm for best quality.
    """
    command = [
        "ffmpeg", "-y", "-i", input_path,
        "-vf", "scale=iw*2:ih*2:flags=lanczos",
        "-c:a", "copy",  # Copy audio if present
        output_path
    ]
    run_ffmpeg(command, "Upscaling video")

def color_grade_video(input_path, output_path):
    """
    Apply color grading: increase contrast, saturation, and brightness for a vibrant look.
    """
    command = [
        "ffmpeg", "-y", "-i", input_path,
        "-vf", "eq=contrast=1.25:saturation=1.6:brightness=0.07",
        "-c:a", "copy",
        output_path
    ]
    run_ffmpeg(command, "Applying color grading")

def add_glow_effect(input_path, output_path):
    """
    Add a soft glow effect for emphasis and a premium feel.
    """
    command = [
        "ffmpeg", "-y", "-i", input_path,
        "-filter_complex", "[0:v]boxblur=15:1[blur];[0:v][blur]blend=all_mode='lighten':opacity=0.5",
        "-c:a", "copy",
        output_path
    ]
    run_ffmpeg(command, "Adding glow effect")

def main():
    print("=== ScoutVision Logo Enhancement ===")
    upscale_video(INPUT_PATH, UPSCALED_PATH)
    color_grade_video(UPSCALED_PATH, GRADED_PATH)
    add_glow_effect(GRADED_PATH, ENHANCED_PATH)
    print(f"\n[COMPLETE] Enhanced logo video saved to: {ENHANCED_PATH}")

if __name__ == "__main__":
    main()
