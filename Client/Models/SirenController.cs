// Five Siren System
// Copyright (c) 2019-2023, Christopher M, Inferno Collection. All rights reserved.
// This project is licensed under the following:
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to use, copy, modify, and merge the software, under the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// The software may not be sold in any format.
// Modified copies of the software may only be shared in an uncompiled format.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace InfernoCollection.Siren.Client.Models
{
    public class SirenController
    {
        public string[] Prefix { get; set; } = null;
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public string Tertiary { get; set; } = "VEHICLES_HORNS_POLICE_WARNING";
        public string Horn { get; set; } = "SIRENS_AIRHORN";
        public bool HornOverlap { get; set; } = false;
        public string Powercall { get; set; } = null;
        public string[] SpecificModels { get; set; } = null;
    }
}