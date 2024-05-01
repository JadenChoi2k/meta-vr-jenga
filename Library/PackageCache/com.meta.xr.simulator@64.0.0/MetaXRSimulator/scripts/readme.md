# vrs_pixmatch.py

This script is designed for comparing screenshots within two VRS files, particularly useful after conducting automated tests to verify whether the test results align with your expectations.

## Usage:

vrs_pixmatch.py [-h] [--threshold THRESHOLD] [--sample_location SAMPLE_LOCATION]
            [--max_test_frames MAX_TEST_FRAMES] [--show_matches SHOW_MATCHES]
            [--diffs_output_path DIFFS_OUTPUT_PATH]
            [--best_match_pixels_diff_threshold BEST_MATCH_PIXELS_DIFF_THRESHOLD]
            record replay

use pixelmatch to diff vrs files

positional arguments:
  record                VRS reference recording file to compare against
  replay                VRS replay to check

optional arguments:
  -h, --help            show this help message and exit
  --threshold THRESHOLD
                        threshold over which the pixels are considered to be different. Rrom 0 to 1. Increasing this number will make the comparison more sensitive. 0.1 is the default value. To delve into details, please review the threshold parameter in the pixelmatch-py library: https://pypi.org/project/pixelmatch/.
  --sample_location SAMPLE_LOCATION
                        where in the group to test: 0=beginning, 0.5=middle, 1=end
  --max_test_frames MAX_TEST_FRAMES
                        max number of matches to perform in each direction, 3 by default
  --show_matches SHOW_MATCHES
                        display the corresponding screenshots side-by-side once a match is identified
  --diffs_output_path DIFFS_OUTPUT_PATH
                        output folder for image diffs
  --best_match_pixels_diff_threshold BEST_MATCH_PIXELS_DIFF_THRESHOLD
                        threshold on the pixels diff of the best match, 100 by default. Recommendation: Fine-tune this parameter alongside --diffs_output_path to determine the optimal value for your specific scenario.

## Sample command:
```
python3 vrs_pixmatch.py new.vrs expect.vrs --threshold 0.2 --diffs_output_path ./
```
