using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// viscoreitano

namespace ANVI6NoanetRegistroDePersonal
{
    public sealed class ViscoreitanoModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class ViscoreitanoModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IList<IDictionary<string, float>> loss { get; set; }
        public ViscoreitanoModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new List<IDictionary<string, float>>();
        }
    }

    public sealed class ViscoreitanoModel
    {
        private LearningModelPreview learningModel;
        public static async Task<ViscoreitanoModel> CreateViscoreitanoModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            ViscoreitanoModel model = new ViscoreitanoModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<ViscoreitanoModelOutput> EvaluateAsync(ViscoreitanoModelInput input) {
            ViscoreitanoModelOutput output = new ViscoreitanoModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
