# ImageFxFramework

**ImageFxFramework** is a configurable image processing pipeline built in C# using [.NET](https://dotnet.microsoft.com/) and [ImageSharp](https://github.com/SixLabors/ImageSharp). It allows you to apply multiple effects such as **resize**, **blur**, and **grayscale** to images based on a JSON configuration.

---

## Features

- Load images in common formats (JPEG, PNG, BMP, etc.).
- Apply multiple effects dynamically:
  - **Resize** – scale images to a specific number of pixels.
  - **Blur** – apply Gaussian blur with configurable radius.
  - **Grayscale** – convert images to grayscale.
- JSON-based configuration for specifying image processing jobs.
- Command-line execution with selection of images to process.
- Outputs processed images with `_out` suffix in the filename.

---

## JSON Configuration

The application uses a JSON configuration to define image processing jobs. Example:

```json
{
  "images": [
    {
      "name": "Image#1.jpg",
      "effects": [
        { "name": "resize", "parameter": { "TargetPixels": 100 } },
        { "name": "blur", "parameter": { "Radius": 2 } }
      ]
    },
    {
      "name": "Image#2.jpg",
      "effects": [
        { "name": "resize", "parameter": { "TargetPixels": 100 } }
      ]
    },
    {
      "name": "Image#3.jpg",
      "effects": [
        { "name": "resize", "parameter": { "TargetPixels": 150 } },
        { "name": "blur", "parameter": { "Radius": 5 } },
        { "name": "grayscale" }
      ]
    }
  ]
}
## Running the Application

Place your images in the project folder and run the application with the filenames as arguments:

dotnet run -- Image#1.jpg Image#2.jpg Image#3.jpg

## Example Output

Processed images:

Image#1_out.jpg
Image#2_out.jpg
Image#3_out.jpg
